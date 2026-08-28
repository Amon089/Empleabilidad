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

        var tenantA = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == "leggumbres-la-escoba");
        var tenantB = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == "todo-metal");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        // ----------------------------------------------------
        // TENANT A - Leggumbres La Escoba
        // ----------------------------------------------------
        if (tenantA == null)
        {
            tenantA = new Tenant
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
            context.Tenants.Add(tenantA);

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
            context.Users.AddRange(userA1, userA2);
        }

        // ----------------------------------------------------
        // TENANT B - Estructuras y Montajes Todo Metal SAS
        // ----------------------------------------------------
        if (tenantB == null)
        {
            tenantB = new Tenant
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
            context.Tenants.Add(tenantB);

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
            context.Users.AddRange(userB1, userB2);
        }

        await context.SaveChangesAsync();

        // ----------------------------------------------------
        // REFRESH & SEED ARTICLES FOR TENANT A (Leggumbres)
        // ----------------------------------------------------
        var existingArticlesA = await context.KnowledgeBaseArticles
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == tenantA.Id)
            .ToListAsync();
        context.KnowledgeBaseArticles.RemoveRange(existingArticlesA);

        var articlesDataA = new List<(string Title, string Content)>
        {
            (
                "informacion_empresa",
                "Leggumbres La Escoba es una empresa dedicada a conectar directamente a los campesinos productores con los hogares de los consumidores. Funciona como centro de acopio y distribución de productos agrícolas. El objetivo principal es reducir intermediarios y facilitar que los productos cultivados por campesinos lleguen frescos y directamente a las familias."
            ),
            (
                "productos",
                "Comercializamos productos agrícolas frescos de campesinos y proveedores asociados: Papa, Yuca, Plátano (verde, pintón, maduro), Tomate (chonto y milano), Cebolla (cabezona blanca/roja y junca/rama), Zanahoria, Fríjol, Lentejas, Arvejas, Maíz, Habichuela, Lechuga, Espinaca, Ajo, Aguacate (Hass y papelillo), Frutas (fresa, papaya, piña oro miel, mango, maracuyá, lulo, granadilla, limón Tahití) y productos de temporada. La disponibilidad cambia según la producción campesina y existencias en el centro de acopio."
            ),
            (
                "pedidos_y_entregas",
                "Los clientes pueden consultar la disponibilidad y realizar pedidos a domicilio directamente desde el sitio web oficial. El centro de acopio prepara los productos frescos para su distribución. Tiempos y horarios: atención y despachos de lunes a sábado de 6:00 AM a 2:00 PM. Opción de 'Recogida en Centro de Acopio' en Bodega 12 de la Zona Agroindustrial Central sin costo de envío de 8:00 AM a 4:00 PM."
            ),
            (
                "pqrs",
                "El sistema de PQRS está disponible para presentar solicitudes, quejas, reclamos o felicitaciones. Casos aplicables: producto recibido en mal estado o magullado, producto faltante en un pedido, entrega incorrecta, retraso en el domicilio, solicitudes de información comercial o felicitaciones al equipo."
            ),
            (
                "preguntas_frecuentes_y_reglas",
                "Instrucciones operativas: El chatbot responde exclusivamente con información de Leggumbres La Escoba. Nunca inventa precios, horarios no registrados, nombres de productores o fincas, ni fechas de entrega individuales. Si la información no está en la base de conocimientos, se debe declarar la falta de información y ofrecer radicar una PQRS."
            )
        };

        foreach (var data in articlesDataA)
        {
            var emb = await aiService.GenerateEmbeddingAsync($"{data.Title}\n{data.Content}");
            context.KnowledgeBaseArticles.Add(new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Title = data.Title,
                Content = data.Content,
                Embedding = new Vector(emb),
                IsActive = true
            });
        }

        // ----------------------------------------------------
        // REFRESH & SEED ARTICLES FOR TENANT B (Todo Metal)
        // ----------------------------------------------------
        var existingArticlesB = await context.KnowledgeBaseArticles
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == tenantB.Id)
            .ToListAsync();
        context.KnowledgeBaseArticles.RemoveRange(existingArticlesB);

        var articlesDataB = new List<(string Title, string Content)>
        {
            (
                "informacion_empresa",
                "Estructuras y Montajes Todo Metal SAS es una empresa dedicada al diseño, fabricación, montaje y ejecución de proyectos de estructuras metálicas y obras de infraestructura pública y privada, incluyendo contratos con entidades gubernamentales y gobernaciones."
            ),
            (
                "servicios",
                "Desarrollamos proyectos de: fabricación y montaje de estructuras metálicas, puentes vehiculares y peatonales, cubiertas industriales, naves comerciales, bodegas, centros logísticos, obras de infraestructura y soluciones estructurales a medida según los requerimientos técnicos del cliente."
            ),
            (
                "estructuras_y_puentes",
                "Participamos en proyectos que requieren estructuras metálicas fabricadas en taller, diseño, transporte y montaje en sitio. Cumplimos con la norma sismorresistente NSR-10 y soldadura AWS D1.1. Los proyectos de puentes e infraestructura requieren análisis de ubicación, cargas, terreno y pliegos contractuales. No entregamos cálculos estructurales definitivos por chat."
            ),
            (
                "proyectos_publicos_y_cotizaciones",
                "Participamos en licitaciones y contratos públicos con gobernaciones y alcaldías. Cada contrato tiene alcances y fechas específicas. Cotizaciones: Dependen del tipo de estructura, dimensiones, tipo de acero, volumen, ubicación del proyecto, transporte y montaje. Sin estos datos no se entrega un precio oficial por chat."
            ),
            (
                "pqrs_y_reglas",
                "Sistema de PQRS para Todo Metal SAS: Los clientes y contratistas pueden radicar PQRS por retrasos en proyectos, solicitudes de información contractual o técnica, problemas de montaje o entregas, quejas o felicitaciones. Reglas: Responder únicamente sobre Todo Metal SAS, nunca inventar contratos, valores, dimensiones, cargas o normas no registradas, y ofrecer radicar una PQRS si la consulta requiere revisión de un ingeniero o empleado."
            )
        };

        foreach (var data in articlesDataB)
        {
            var emb = await aiService.GenerateEmbeddingAsync($"{data.Title}\n{data.Content}");
            context.KnowledgeBaseArticles.Add(new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Title = data.Title,
                Content = data.Content,
                Embedding = new Vector(emb),
                IsActive = true
            });
        }

        foreach (var data in articlesDataB)
        {
            var emb = await aiService.GenerateEmbeddingAsync($"{data.Title}\n{data.Content}");
            context.KnowledgeBaseArticles.Add(new KnowledgeBaseArticle
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Title = data.Title,
                Content = data.Content,
                Embedding = new Vector(emb),
                IsActive = true
            });
        }

        // Seed Tickets if missing
        if (!await context.Tickets.AnyAsync())
        {
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

            context.Tickets.AddRange(ticketsA);
            context.Tickets.AddRange(ticketsB);
        }

        await context.SaveChangesAsync();
    }
}
