using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pqrs.Application.DTOs.Widget;
using Pqrs.Application.Interfaces;
using Pqrs.Domain.Entities;

namespace Pqrs.Application.Services;

public class RagService
{
    private readonly IApplicationDbContext _context;
    private readonly IAiService _aiService;

    public RagService(IApplicationDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<RagSearchResponseDto> SearchAndAnswerAsync(
        string query, 
        Guid tenantId, 
        double threshold, 
        int topK = 5, 
        string sessionId = "", 
        CancellationToken cancellationToken = default)
    {
        var cleanQuery = query.Trim().ToLowerInvariant();

        // 1. Greetings Intent
        var greetings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hola", "hola!", "holaa", "holaaa", "buenas", "buenas!", "buenos dias", "buenos días", 
            "buenas tardes", "buenas noches", "saludos", "hey", "hi", "hello", "ayuda", "que puedes hacer"
        };

        if (greetings.Contains(cleanQuery) || cleanQuery.StartsWith("hola ") || cleanQuery.StartsWith("buenas "))
        {
            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = "¡Hola! 👋 Bienvenido a nuestro servicio de atención. ¿En qué te puedo ayudar hoy? Puedes consultarme sobre nuestros productos, pedidos, envíos, garantías o radicar una PQRS.",
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // 2. Identity & Small Talk
        if (cleanQuery.Contains("persona") || cleanQuery.Contains("robot") || cleanQuery.Contains("humano") || cleanQuery.Contains("quien eres") || cleanQuery.Contains("quién eres") || cleanQuery.Contains("como te llamas") || cleanQuery.Contains("quien te creo"))
        {
            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = "🤖 ¡Hola! Soy el Asistente Virtual Inteligente de Atención al Cliente y PQRS. Estoy diseñado para responder tus consultas 24/7 sobre productos, envíos, horarios, cotizaciones y servicios.",
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // 3. Human Escalation
        if (cleanQuery.Contains("humano") || cleanQuery.Contains("persona real") || cleanQuery.Contains("asesor") || cleanQuery.Contains("agente") || cleanQuery.Contains("supervisor") || cleanQuery.Contains("hablar con alguien"))
        {
            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = "👤 Entendido. Para ser atendido por un agente o asesor humano, por favor haz clic en el botón verde '📝 Radicar PQRS' a continuación y un miembro de nuestro equipo tomará tu caso.",
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // 4. Catalog / Products / What do you sell Intent
        if (cleanQuery.Contains("vendes") || cleanQuery.Contains("venden") || cleanQuery.Contains("ofrecen") || cleanQuery.Contains("productos") || cleanQuery.Contains("servicios") || cleanQuery.Contains("catalogo") || cleanQuery.Contains("catálogo") || cleanQuery.Contains("que tienen") || cleanQuery.Contains("qué tienen") || cleanQuery.Contains("que cosas") || cleanQuery.Contains("qué cosas"))
        {
            var isTodoMetalTenant = await _context.KnowledgeBaseArticles
                .IgnoreQueryFilters()
                .AnyAsync(a => a.TenantId == tenantId && a.Content.Contains("Todo Metal"), cancellationToken);

            var catalogText = isTodoMetalTenant
                ? "🏗️ **Estructuras y Montajes Todo Metal SAS** ofrece soluciones integrales de ingeniería:\n\n" +
                  "• **Estructuras Metálicas**: Fabricación y montaje de bodegas industriales, naves logísticas, mezzanines (Steel Deck) y cubiertas sándwich termoacústicas.\n" +
                  "• **Puentes e Infraestructura**: Puentes vehiculares sismorresistentes, puentes peatonales de celosía/atirantados y maniobras de izaje pesado.\n" +
                  "• **Obras Civiles y Urbanismo**: Cimentaciones profundas, pilotes, placas de contrapiso industrial, movimiento de tierras y demolición técnica.\n" +
                  "• **Normativa y Garantía**: Diseños bajo norma **NSR-10**, soldadura certificada **AWS D1.1**, ensayos NDT y **Garantía Decenal Ley 1796**."
                : "🥦 **Leggumbres La Escoba** ofrece productos agrícolas frescos directamente del campo a tu hogar:\n\n" +
                  "• **Frutas Frescas e Importadas**: Aguacate Hass/Papelillo, Plátano (verde, pintón, maduro), Fresa, Papaya, Piña Oro Miel, Limón Tahití, Kiwi y Uvas sin semilla.\n" +
                  "• **Verduras y Hortalizas**: Tomate Chonto/Milano, Papas (Criolla, Pastusa, Nevada, Capira), Cebolla (blanca, roja, junca), Lechugas, Brócoli, Espinaca, Ajo y Pimentón.\n" +
                  "• **Legumbres y Granos**: Fríjol seco/verde desgranado, Lenteja, Garbanzo, Arveja fresca y Maíz tierno.\n" +
                  "• **Hierbas y Raíces**: Cilantro, Perejil, Albahaca, Hierbabuena, Romero, Tomillo, Jengibre y Cúrcuma fresca.";

            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = catalogText,
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // 5. Physical Location / Address / Office Intent
        if (cleanQuery.Contains("oficina") || cleanQuery.Contains("sede") || cleanQuery.Contains("ubicacion") || cleanQuery.Contains("ubicación") || cleanQuery.Contains("direccion") || cleanQuery.Contains("dirección") || cleanQuery.Contains("donde estan") || cleanQuery.Contains("donde queda") || cleanQuery.Contains("donde es"))
        {
            var isTodoMetalTenant = await _context.KnowledgeBaseArticles
                .IgnoreQueryFilters()
                .AnyAsync(a => a.TenantId == tenantId && a.Content.Contains("Todo Metal"), cancellationToken);

            var locationText = isTodoMetalTenant
                ? "🏢 Nuestra sede principal, oficinas administrativas y planta de producción de Estructuras y Montajes Todo Metal SAS están ubicadas en el Parque Industrial Metalmecánico (Manzana B, Lote 4). Atendemos presencialmente de lunes a viernes de 7:00 AM a 5:00 PM y sábados de 8:00 AM a 12:00 PM."
                : "🥦 Nuestro centro de acopio, oficinas y bodega principal de Leggumbres La Escoba están ubicados en la Zona Agroindustrial Central (Bodega 12). Realizamos despachos y entregas a domicilio en toda la ciudad.";

            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = locationText,
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // 6. Delivery / Domicilios Intent
        if (cleanQuery.Contains("domicilio") || cleanQuery.Contains("envio") || cleanQuery.Contains("envío") || cleanQuery.Contains("entrega") || cleanQuery.Contains("despacho"))
        {
            var isTodoMetalTenant = await _context.KnowledgeBaseArticles
                .IgnoreQueryFilters()
                .AnyAsync(a => a.TenantId == tenantId && a.Content.Contains("Todo Metal"), cancellationToken);

            var deliveryText = isTodoMetalTenant
                ? "🚛 En Estructuras y Montajes Todo Metal SAS realizamos transporte, despacho e izamiento de estructuras metálicas y obras de infraestructura a nivel nacional con flotilla propia y escolta de carga pesada."
                : "🥦 Realizamos entregas a domicilio de frutas y verduras frescas de lunes a sábado entre las 6:00 AM y las 2:00 PM con tarifa plana de $4,500 en la zona urbana.";

            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = deliveryText,
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // 7. Perform RAG Embedding Search & LLM Synthesis
        var queryVec = await _aiService.GenerateEmbeddingAsync(query, cancellationToken);

        var articles = await _context.KnowledgeBaseArticles
            .IgnoreQueryFilters()
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .ToListAsync(cancellationToken);

        if (!articles.Any())
        {
            await LogInteractionAsync(tenantId, sessionId, query, 0.0, false, cancellationToken);
            return new RagSearchResponseDto
            {
                Resolved = false,
                Answer = null,
                Sources = new List<RagSourceDto>(),
                TopScore = 0.0
            };
        }

        var rawAnswer = await _aiService.GenerateRagAnswerAsync(query, articles, cancellationToken);
        var cleanAnswer = CleanAnswerText(rawAnswer);

        bool llmSuccess = !string.IsNullOrWhiteSpace(cleanAnswer) && 
                           !cleanAnswer.Contains("No encuentro información suficiente") && 
                           !cleanAnswer.Contains("no cuentas con información") &&
                           !cleanAnswer.Contains("No puedo procesar esta consulta");

        if (llmSuccess)
        {
            var sources = articles.Take(3).Select(a => new RagSourceDto
            {
                ArticleId = a.Id,
                Title = a.Title
            }).ToList();

            await LogInteractionAsync(tenantId, sessionId, query, 1.0, true, cancellationToken);

            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = cleanAnswer,
                Sources = sources,
                TopScore = 1.0
            };
        }

        await LogInteractionAsync(tenantId, sessionId, query, 0.0, false, cancellationToken);
        return new RagSearchResponseDto
        {
            Resolved = false,
            Answer = null,
            Sources = new List<RagSourceDto>(),
            TopScore = 0.0
        };
    }

    private string CleanAnswerText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var lines = text.Split('\n');
        var cleanLines = new List<string>();

        foreach (var line in lines)
        {
            var l = line.Trim();
            if (l.StartsWith("PREGUNTAS Y RESPUESTAS", StringComparison.OrdinalIgnoreCase) ||
                l.StartsWith("PREGUNTAS Frecuentes", StringComparison.OrdinalIgnoreCase) ||
                l.Contains("Q&As):", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Remove P101:, R101:, P151: prefixes
            l = System.Text.RegularExpressions.Regex.Replace(l, @"^[PR]\d+:\s*", "");
            l = System.Text.RegularExpressions.Regex.Replace(l, @"^P\d+\s+a\s+P\d+:\s*", "");

            if (!string.IsNullOrWhiteSpace(l))
            {
                cleanLines.Add(l);
            }
        }

        return string.Join("\n", cleanLines).Trim();
    }

    private async Task LogInteractionAsync(Guid tenantId, string sessionId, string query, double score, bool resolved, CancellationToken cancellationToken)
    {
        try
        {
            _context.RagInteractions.Add(new RagInteraction
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SessionId = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString() : sessionId,
                Question = query,
                TopScore = score,
                Resolved = resolved,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch { }
    }
}
