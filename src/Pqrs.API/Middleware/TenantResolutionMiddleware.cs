using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pqrs.Application.Interfaces;
using Pqrs.Infrastructure;

namespace Pqrs.API.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantSetter tenantSetter, IApplicationDbContext dbContext)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Skip middleware for OPTIONS preflight requests, swagger, root, healthcheck
        if (HttpMethods.IsOptions(context.Request.Method) || path.StartsWith("/swagger") || path == "/" || path.StartsWith("/health") || path.StartsWith("/api/v1/hubs"))
        {
            await _next(context);
            return;
        }

        // 1. Widget Endpoints: Resolve via X-Widget-Key header
        if (path.StartsWith("/api/v1/widget"))
        {
            if (!context.Request.Headers.TryGetValue("X-Widget-Key", out var widgetKeyHeader) || string.IsNullOrWhiteSpace(widgetKeyHeader))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                var errorResponse = new { error = new { code = "MISSING_WIDGET_KEY", message = "X-Widget-Key header is required." } };
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                return;
            }

            var widgetKey = widgetKeyHeader.ToString().Trim();

            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.WidgetPublicKey == widgetKey && t.IsActive);

            if (tenant == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var errorResponse = new { error = new { code = "INVALID_WIDGET_KEY", message = "Invalid or inactive widget key." } };
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                return;
            }

            tenantSetter.SetTenantId(tenant.Id);
            context.Items["Tenant"] = tenant;
        }
        // 2. Authenticated Endpoints: Resolve via JWT tenant_id claim
        else if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                tenantSetter.SetTenantId(tenantId);
            }
        }

        await _next(context);
    }
}
