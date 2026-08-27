using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pqrs.Domain.Entities;
using Pqrs.Domain.Interfaces;

namespace Pqrs.API.Middleware;

public class DynamicCorsMiddleware
{
    private readonly RequestDelegate _next;

    public DynamicCorsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.Request.Headers.TryGetValue("Origin", out var originHeader) && !string.IsNullOrWhiteSpace(originHeader))
        {
            var requestOrigin = originHeader.ToString().TrimEnd('/');

            if (context.Items.TryGetValue("Tenant", out var tenantObj) && tenantObj is Tenant tenant)
            {
                var allowed = tenant.AllowedOrigins.Select(o => o.TrimEnd('/')).ToList();
                if (allowed.Contains(requestOrigin, System.StringComparer.OrdinalIgnoreCase))
                {
                    context.Response.Headers["Access-Control-Allow-Origin"] = requestOrigin;
                    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Widget-Key";
                    context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, PATCH, DELETE, OPTIONS";
                    context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                }
                else if (context.Request.Method == HttpMethods.Options)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }
            else
            {
                // General CORS headers for non-tenant specific routes or pre-resolution OPTIONS
                context.Response.Headers["Access-Control-Allow-Origin"] = requestOrigin;
                context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Widget-Key";
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, PATCH, DELETE, OPTIONS";
            }
        }

        if (context.Request.Method == HttpMethods.Options)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            return;
        }

        await _next(context);
    }
}
