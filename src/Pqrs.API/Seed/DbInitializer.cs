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
                "Cómo realizar un pedido y canales de atención",
                "Puedes realizar tus pedidos de frutas y verduras frescas directamente a través de nuestro sitio web oficial. Nuestro centro de acopio recibe los productos directamente de los campesinos locales, los clasifica, pesa y empaca en empaques biodegradables para entrega a domicilio."
            ),
            (
                "Zonas de cobertura, envíos y horarios de entrega",
                "Realizamos entregas a domicilio en toda la Ciudad Principal y Municipios aledaños. Entregamos de lunes a sábado entre las 6:00 AM y las 2:00 PM. Los pedidos realizados antes de las 5:00 PM se entregan a primera hora del día siguiente."
            ),
            (
                "Garantía de frescura, cambios y devoluciones",
                "Garantizamos el 100% de la frescura de nuestros productos del campo. Si algún producto llega en mal estado, magullado o incompleto, puedes solicitar el cambio sin costo adicional o reembolso enviando una foto dentro de las 24 horas siguientes a la entrega a través del chat o sistema PQRS."
            ),
            (
                "Catálogo de productos frescos y ofertas de temporada",
                "Contamos con oferta permanente y de temporada: Papa sabanera, Papa criolla, Yuca, Plátano verde y maduro, Fríjol cargamanto, Lentejas, Arveja, Tomate chonto, Cebolla cabezona y larga, Zanahoria, Aguacate hass, Mango, Papaya, Naranja y Limón tahití."
            ),
            (
                "Medios de pago aceptados",
                "Aceptamos pago en efectivo contra entrega, transferencias electrónicas a través de Nequi, Daviplata, Bancolombia, así como tarjetas de crédito y débito mediante nuestra pasarela de pagos segura en el sitio web."
            ),
            (
                "Horarios de atención y días de servicio",
                "Ofrecemos atención al cliente y servicio de entregas a domicilio de lunes a sábado de 6:00 AM a 2:00 PM. ¿Qué días tienen servicio o atención? Atendemos y realizamos despachos de lunes a sábado."
            ),
            (
                "Ubicación de oficinas, sede principal y dirección de acopio",
                "¿Dónde está la oficina o bodega de Leggumbres La Escoba? Nuestra sede principal, oficinas de atención y centro de acopio están ubicados en la Zona Agroindustrial Central (Bodega 12). Realizamos envíos a domicilio en toda la Ciudad Principal y municipios aledaños."
            ),
            (
                "Recogida en centro de acopio, retiro en punto físico y bodega",
                "¿Puedo ir por mi pedido o recogerlo en la central/bodega? ¡Sí, claro! Puedes realizar tu pedido a través de nuestro sitio web seleccionando la opción 'Recogida en Centro de Acopio' y pasar a retirarlo personalmente en nuestra bodega de la Zona Agroindustrial Central (Bodega 12) de lunes a sábado entre las 8:00 AM y las 4:00 PM sin ningún costo de envío."
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
                "Servicios de estructuras metálicas e infraestructura",
                "Estructuras y Montajes Todo Metal SAS se especializa en diseño, cálculo estructural, fabricación, montaje y mantenimiento preventivo y correctivo de estructuras de acero, puentes vehiculares y peatonales, cubiertas industriales y naves comerciales para alcaldías, gobernaciones y sector privado."
            ),
            (
                "Solicitud de cotizaciones, planos y visitas técnicas",
                "Para solicitar una cotización o visita técnica en sitio, debes adjuntar los planos de arquitectura e ingeniería e incluir el pliego de condiciones enviándolo al correo proyectos@todometal.local o radicando una solicitud en nuestro portal. Un ingeniero especialista programará la visita técnica en un plazo máximo de 48 horas hábiles."
            ),
            (
                "Garantía legal y protocolo postventa estructural",
                "Todas nuestras obras y estructuras cuentan con una garantía legal de 10 años en elementos estructurales principales y 5 años en pintura anticorrosiva y uniones soldadas según la norma NSR-10. Ante cualquier novedad de corrosión, fisura o desajuste de pernos, enviamos una cuadrilla de inspección prioritaria en menos de 24 horas."
            ),
            (
                "Certificaciones de calidad, normas NSR-10 y AWS D1.1",
                "Cumplimos rigurosamente con el Reglamento Colombiano de Construcción Sismo Resistente NSR-10 y la norma internacional AWS D1.1 de soldadura en estructuras de acero. Todo nuestro personal de soldadores cuenta con certificación vigente y realizamos Ensayos No Destructivos (END) por Ultrasonido y Tintas Penetrantes."
            ),
            (
                "Formas de pago y condiciones comerciales de proyectos",
                "Trabajamos bajo esquemas de contratación por avance de obra: 50% de anticipo al firmar contrato para adquisición de perfiles de acero, 40% según actas parciales de avance de fabricación y montaje, y 10% restante a la firma del acta de recibo final a satisfacción."
            ),
            (
                "Horarios de atención, días de servicio y canales de atención",
                "Prestamos servicio de atención comercial, técnica e ingenieril de lunes a viernes de 7:00 AM a 5:00 PM y sábados de 8:00 AM a 12:00 PM. ¿Qué días tienen servicio o atención? Ofrecemos servicio de lunes a sábado. Puedes realizar tus solicitudes a través del portal de atención o escribiendo a proyectos@todometal.local."
            ),
            (
                "Ubicación de oficinas, sede principal y planta industrial",
                "¿Dónde está la oficina o sede de Estructuras y Montajes Todo Metal SAS? Nuestra sede principal, oficinas administrativas y planta de producción están ubicadas en el Parque Industrial Metalmecánico (Manzana B, Lote 4). Atendemos de lunes a viernes de 7:00 AM a 5:00 PM y sábados de 8:00 AM a 12:00 PM."
            ),
            (
                "Retiro de materiales, perfiles y recogida en planta industrial",
                "¿Puedo ir por materiales o recoger elementos en la planta? Sí, contratistas y clientes pueden enviar vehículos autorizados para retirar elementos fabricados, perfiles de acero o estructuras en nuestra planta industrial de lunes a viernes de 7:00 AM a 4:00 PM presentando la orden de despacho o contrato firmado."
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
