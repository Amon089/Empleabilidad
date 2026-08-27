using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pqrs.Application.Interfaces;
using Pqrs.Domain.Entities;
using Pqrs.Domain.Enums;
using Pqrs.Infrastructure.Persistence;

namespace Pqrs.API.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(PqrsDbContext context, IAiService aiService)
    {
        // Ensure Database schema is created
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        if (await context.Tenants.AnyAsync())
        {
            return; // DB has been seeded already
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        // ----------------------------------------------------
        // TENANT A - Leggumbres La Escoba
        // ----------------------------------------------------
        var tenantA = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Leggumbres La Escoba",
            Slug = "leggumbres-la-escoba",
            WidgetPublicKey = "pk_live_escoba_12345",
            AllowedOrigins = new List<string>
            {
                "https://leggumbres-la-escoba.local",
                "https://www.leggumbres-la-escoba.local"
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var userA1 = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantA.Id,
            Name = "Admin Leggumbres",
            Email = "admin@leggumbres.local",
            PasswordHash = passwordHash,
            Role = UserRole.ADMIN,
            IsActive = true
        };

        var userA2 = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantA.Id,
            Name = "Agente Leggumbres",
            Email = "agent@leggumbres.local",
            PasswordHash = passwordHash,
            Role = UserRole.AGENT,
            IsActive = true
        };

        var kbA1Content = "Los pedidos se realizan a traves de nuestra plataforma web o WhatsApp. El centro de acopio recibe los productos de los campesinos, los clasifica y los empaca para envio.";
        var kbA2Content = "Zonas de cobertura: Ciudad Principal y Municipios aledanos. Horarios de entrega: lunes a sabado de 6:00 AM a 2:00 PM.";
        var kbA3Content = "Politica de devoluciones: Si un producto llega en mal estado o incompleto, se realiza el cambio o reembolso reportando dentro de las 24 horas posteriores a la entrega.";
        var kbA4Content = "Ofrecemos Papa, Yuca, Platano, Frijol, Lentejas, Arveja, Tomate, Cebolla, Zanahoria y Aguacate frescos.";

        var articlesA = new List<KnowledgeBaseArticle>
        {
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Title = "Como realizar un pedido y entregas",
                Content = kbA1Content,
                Embedding = new Vector(await aiService.GenerateEmbeddingAsync($"Como realizar un pedido y entregas\n{kbA1Content}")),
                IsActive = true
            },
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Title = "Zonas de cobertura y horarios de entrega",
                Content = kbA2Content,
                Embedding = new Vector(await aiService.GenerateEmbeddingAsync($"Zonas de cobertura y horarios de entrega\n{kbA2Content}")),
                IsActive = true
            },
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Title = "Politica de cambios y devoluciones en mal estado",
                Content = kbA3Content,
                Embedding = new Vector(await aiService.GenerateEmbeddingAsync($"Politica de cambios y devoluciones en mal estado\n{kbA3Content}")),
                IsActive = true
            },
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Title = "Productos disponibles y de temporada",
                Content = kbA4Content,
                Embedding = new Vector(await aiService.GenerateEmbeddingAsync($"Productos disponibles y de temporada\n{kbA4Content}")),
                IsActive = true
            }
        };

        var ticketsA = new List<Ticket>
        {
            new Ticket
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                CustomerName = "Carlos Gomez",
                CustomerEmail = "carlos@example.com",
                Subject = "Mi pedido llego incompleto",
                Description = "Mi pedido llego incompleto, no recibi la yuca ni las papas.",
                Type = TicketType.CLAIM,
                Status = TicketStatus.IN_PROGRESS,
                Priority = Priority.HIGH,
                Sentiment = Sentiment.NEGATIVE,
                Summary = "Cliente informa pedido incompleto sin papas ni yuca.",
                ResolvedByRag = false
            },
            new Ticket
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                CustomerName = "Maria Rodriguez",
                CustomerEmail = "maria@example.com",
                Subject = "Las papas llegaron en mal estado",
                Description = "Las papas venian magulladas y en mal estado.",
                Type = TicketType.CLAIM,
                Status = TicketStatus.PENDING,
                Priority = Priority.HIGH,
                Sentiment = Sentiment.NEGATIVE,
                Summary = "Reclamacion por producto en mal estado.",
                ResolvedByRag = false
            }
        };

        // ----------------------------------------------------
        // TENANT B - Estructuras y Montajes Todo Metal SAS
        // ----------------------------------------------------
        var tenantB = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Estructuras y Montajes Todo Metal SAS",
            Slug = "todo-metal",
            WidgetPublicKey = "pk_live_todometal_67890",
            AllowedOrigins = new List<string>
            {
                "https://todo-metal.local",
                "https://www.todo-metal.local"
            },
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var userB1 = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantB.Id,
            Name = "Admin Todo Metal",
            Email = "admin@todometal.local",
            PasswordHash = passwordHash,
            Role = UserRole.ADMIN,
            IsActive = true
        };

        var userB2 = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantB.Id,
            Name = "Agente Todo Metal",
            Email = "agent@todometal.local",
            PasswordHash = passwordHash,
            Role = UserRole.AGENT,
            IsActive = true
        };

        var kbB1Content = "Ofrecemos diseno, fabricacion, montaje y mantenimiento de estructuras metalicas, puentes y obras de infraestructura para gobernaciones y entidades.";
        var kbB2Content = "Para solicitar cotizacion o visita tecnica, se debe enviar la documentacion requerida del proyecto a nuestro canal de atencion de contratacion.";
        var kbB3Content = "Garantias y postventa: Todas nuestras estructuras cuentan con garantia legal de 10 anos en elementos estructurales. Ante reportes de corrosion o problemas estructurales se programa visita prioritaria.";

        var articlesB = new List<KnowledgeBaseArticle>
        {
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Title = "Servicios de estructuras metalicas y contratacion",
                Content = kbB1Content,
                Embedding = new Vector(await aiService.GenerateEmbeddingAsync($"Servicios de estructuras metalicas y contratacion\n{kbB1Content}")),
                IsActive = true
            },
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Title = "Solicitud de cotizaciones y visita tecnica",
                Content = kbB2Content,
                Embedding = new Vector(await aiService.GenerateEmbeddingAsync($"Solicitud de cotizaciones y visita tecnica\n{kbB2Content}")),
                IsActive = true
            },
            new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Title = "Garantias y reporte de problemas estructurales",
                Content = kbB3Content,
                Embedding = new Vector(await aiService.GenerateEmbeddingAsync($"Garantias y reporte de problemas estructurales\n{kbB3Content}")),
                IsActive = true
            }
        };

        var ticketsB = new List<Ticket>
        {
            new Ticket
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                CustomerName = "Gobernacion de Antioquia",
                CustomerEmail = "obras@antioquia.gov.co",
                Subject = "Necesito una visita tecnica",
                Description = "Requerimos inspeccion en sitio para el montaje del nuevo puente peatonal.",
                Type = TicketType.PETITION,
                Status = TicketStatus.PENDING,
                Priority = Priority.MEDIUM,
                Sentiment = Sentiment.NEUTRAL,
                Summary = "Solicitud de visita tecnica para evaluacion de proyecto.",
                ResolvedByRag = false
            },
            new Ticket
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                CustomerName = "Ing. Roberto Martinez",
                CustomerEmail = "roberto@infraestructura.com",
                Subject = "El puente presenta un problema en una union",
                Description = "Se evidencia una ligera corrosion en los pernos de la union 4B del puente vehicular.",
                Type = TicketType.CLAIM,
                Status = TicketStatus.IN_PROGRESS,
                Priority = Priority.HIGH,
                Sentiment = Sentiment.NEGATIVE,
                Summary = "Reporte de problema estructural por corrosion en union.",
                ResolvedByRag = false
            }
        };

        context.Tenants.AddRange(tenantA, tenantB);
        context.Users.AddRange(userA1, userA2, userB1, userB2);
        context.KnowledgeBaseArticles.AddRange(articlesA);
        context.KnowledgeBaseArticles.AddRange(articlesB);
        context.Tickets.AddRange(ticketsA);
        context.Tickets.AddRange(ticketsB);

        await context.SaveChangesAsync();
    }
}
