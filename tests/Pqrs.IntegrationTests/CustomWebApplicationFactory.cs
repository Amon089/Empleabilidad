using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Pqrs.API.Seed;
using Pqrs.Application.Interfaces;
using Pqrs.Infrastructure.Persistence;

namespace Pqrs.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("UseInMemoryDatabase", "true");

        builder.ConfigureServices(services =>
        {
            // Seed database after application host built
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PqrsDbContext>();
            var aiService = scope.ServiceProvider.GetRequiredService<IAiService>();

            db.Database.EnsureCreated();
            DbInitializer.SeedAsync(db, aiService).GetAwaiter().GetResult();
        });
    }
}
