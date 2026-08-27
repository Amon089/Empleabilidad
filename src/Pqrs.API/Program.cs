using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Pgvector.EntityFrameworkCore;
using Pqrs.API.BackgroundServices;
using Pqrs.API.Hubs;
using Pqrs.API.Middleware;
using Pqrs.API.Seed;
using Pqrs.API.Services;
using Pqrs.Application.Interfaces;
using Pqrs.Application.Services;
using Pqrs.Domain.Interfaces;
using Pqrs.Infrastructure;
using Pqrs.Infrastructure.Persistence;
using Pqrs.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Database Connection String & Provider Setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? builder.Configuration["ConnectionStrings__Default"] 
                       ?? "Host=localhost;Port=5432;Database=pqrs_saas_db;Username=postgres;Password=postgres";

var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

builder.Services.AddDbContext<PqrsDbContext>((sp, options) =>
{
    if (useInMemory)
    {
        options.UseInMemoryDatabase("PqrsTestDb");
    }
    else
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.UseVector();
        });
    }
});

builder.Services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<PqrsDbContext>());

// Multi-Tenant Context
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
builder.Services.AddScoped<ITenantSetter>(sp => sp.GetRequiredService<TenantContext>());

// Infrastructure & Application Services
builder.Services.AddHttpClient<IAiService, AiService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<ITicketQueue, TicketQueue>();
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<TriageService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<KnowledgeBaseService>();

// Hosted Background Service for AI Triage
builder.Services.AddHostedService<TicketTriageBackgroundService>();

// SignalR Hub
builder.Services.AddSignalR();

// Rate Limiter
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("WidgetPolicy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

// Authentication & JWT Bearer
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "SuperSecretKey_For_PQRS_SaaS_Platform_1234567890!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PqrsApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PqrsWidget";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Swagger Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PQRS SaaS Multi-Tenant API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure Middleware Pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<DynamicCorsMiddleware>();

if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PQRS SaaS API v1");
    });
}

// Enable Static Files (Serves /widget/pqrs-widget.js)
app.UseStaticFiles();

app.UseRouting();

// Authentication MUST be executed before TenantResolutionMiddleware so HttpContext.User claims are available
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHub<NotificationsHub>("/api/v1/hubs/notifications");

// Database Seed on Startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<PqrsDbContext>();
        var aiService = scope.ServiceProvider.GetRequiredService<IAiService>();
        await DbInitializer.SeedAsync(db, aiService);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during DB initialization/seeding: {ex.Message}");
    }
}

app.Run();

public partial class Program { }
