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
                Answer = "¡Hola! 👋 Bienvenido a nuestro servicio de atención. ¿En qué te puedo ayudar hoy? Puedes consultarme sobre nuestros productos, pedidos, envíos, garantias o radicar una PQRS.",
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // Identity & Small Talk
        if (cleanQuery.Contains("persona") || cleanQuery.Contains("robot") || cleanQuery.Contains("humano") || cleanQuery.Contains("quien eres") || cleanQuery.Contains("quién eres") || cleanQuery.Contains("como te llamas") || cleanQuery.Contains("quien te creo"))
        {
            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = "🤖 ¡Hola! Soy el Asistente Virtual Inteligente de Atención al Cliente y PQRS. Estoy diseñado con Inteligencia Artificial para responder tus consultas 24/7 sobre productos, envíos, horarios, cotizaciones y servicios.",
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // Human Escalation
        if (cleanQuery.Contains("humano") || cleanQuery.Contains("persona real") || cleanQuery.Contains("asesor") || cleanQuery.Contains("agente") || cleanQuery.Contains("supervisor") || cleanQuery.Contains("hablar con alguien"))
        {
            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = "👤 Entendido. Para ser atendido por un agente o asesor humano, por favor haz clic en el botón '📝 Radicar PQRS' a continuación y un miembro de nuestro equipo tomará tu caso.",
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // 1. Generate query embedding
        var queryVec = await _aiService.GenerateEmbeddingAsync(query, cancellationToken);

        // 2. Fetch active KB articles for current tenant
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

        // 3. Attempt LLM generation with tenant context
        var answer = await _aiService.GenerateRagAnswerAsync(query, articles, cancellationToken);
        bool llmSuccess = !string.IsNullOrWhiteSpace(answer) && 
                           !answer.Contains("No hay información suficiente") && 
                           !answer.Contains("no cuentas con información");

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
                Answer = answer,
                Sources = sources,
                TopScore = 1.0
            };
        }

        // Physical Location / Address / Office intent
        if (cleanQuery.Contains("oficina") || cleanQuery.Contains("sede") || cleanQuery.Contains("ubicacion") || cleanQuery.Contains("ubicación") || cleanQuery.Contains("direccion") || cleanQuery.Contains("dirección") || cleanQuery.Contains("donde estan") || cleanQuery.Contains("donde queda") || cleanQuery.Contains("donde es"))
        {
            var isTodoMetal = articles.Any(a => a.Content.Contains("Todo Metal"));
            var locationText = isTodoMetal
                ? "🏢 Nuestra sede principal, oficinas administrativas y planta de producción de Estructuras y Montajes Todo Metal SAS están ubicadas en el Parque Industrial Metalmecánico (Manzana B, Lote 4). Atendemos presencialmente de lunes a viernes de 7:00 AM a 5:00 PM y sábados de 8:00 AM a 12:00 PM."
                : "🥦 Nuestro centro de acopio, oficinas y bodega principal de Leggumbres La Escoba están ubicados en la Zona Agroindustrial Central (Bodega 12). Realizamos despachos y entregas a domicilio en toda la Ciudad Principal y municipios aledaños.";

            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = locationText,
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // Pickup in Central / Store / Plant intent
        if (cleanQuery.Contains("recoger") || cleanQuery.Contains("ir por") || cleanQuery.Contains("retirar") || cleanQuery.Contains("recogida") || cleanQuery.Contains("punto fisico") || cleanQuery.Contains("punto físico"))
        {
            var isTodoMetal = articles.Any(a => a.Content.Contains("Todo Metal"));
            var pickupText = isTodoMetal
                ? "🚛 Sí, contratistas y clientes pueden retirar materiales o estructuras directamente en nuestra planta industrial (Manzana B, Lote 4) de lunes a viernes de 7:00 AM a 4:00 PM presentando la orden de despacho o contrato."
                : "🥦 ¡Sí, claro! Puedes solicitar tu pedido seleccionando la opción 'Recogida en Centro de Acopio' y pasar a retirarlo directamente en nuestra bodega de la Zona Agroindustrial Central (Bodega 12) de lunes a sábado de 8:00 AM a 4:00 PM sin ningún costo de envío.";

            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = pickupText,
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // Catalog / Products / What do you sell intent
        if (cleanQuery.Contains("vendes") || cleanQuery.Contains("venden") || cleanQuery.Contains("ofrecen") || cleanQuery.Contains("productos") || cleanQuery.Contains("servicios") || cleanQuery.Contains("catalogo") || cleanQuery.Contains("catálogo") || cleanQuery.Contains("que tienen") || cleanQuery.Contains("qué tienen") || cleanQuery.Contains("que cosas") || cleanQuery.Contains("qué cosas"))
        {
            var isTodoMetal = articles.Any(a => a.Content.Contains("Todo Metal"));
            var catalogText = isTodoMetal
                ? "🏗️ En Estructuras y Montajes Todo Metal SAS nos especializamos en: Diseño, fabricación y montaje de estructuras metálicas, puentes vehiculares y peatonales, naves industriales, bodegas, cubiertas, obras de infraestructura y soluciones de construcción sismorresistente NSR-10."
                : "🥦 En Leggumbres La Escoba vendemos productos agrícolas frescos directamente del campo a tu hogar: Papa, Yuca, Plátano (verde, pintón, maduro), Tomate (chonto y milano), Cebolla (cabezona y junca), Zanahoria, Fríjol, Lentejas, Arvejas, Maíz, Habichuela, Lechuga, Espinaca, Ajo, Aguacate (Hass y papelillo), Frutas frescas (fresa, papaya, piña oro miel, mango, maracuyá, lulo, granadilla, limón Tahití) y productos de temporada.";

            return new RagSearchResponseDto
            {
                Resolved = true,
                Answer = catalogText,
                Sources = new List<RagSourceDto>(),
                TopScore = 1.0
            };
        }

        // 4. Fallback Hybrid Scoring when LLM indicates question is out of KB scope
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "que", "cuanto", "cuando", "donde", "dónde", "como", "cómo", "quien", "quién", "los", "las", "del", "por", "para", "con", "sin", "mas", "más", "se", "un", "una", "de" };
        var queryWords = query.ToLowerInvariant()
            .Split(new[] { ' ', '\t', ',', '.', ';', ':', '?', '!', '¿', '¡' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !stopWords.Contains(w))
            .ToList();

        if (!queryWords.Any())
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

        var scoredArticles = new List<(KnowledgeBaseArticle Article, double Score)>();

        foreach (var article in articles)
        {
            double vecScore = 0.0;
            if (article.Embedding != null)
            {
                vecScore = ComputeCosineSimilarity(queryVec, article.Embedding.ToArray());
            }

            double keywordScore = 0.0;
            var fullArticleText = $"{article.Title} {article.Content}".ToLowerInvariant();
            int matchedCount = 0;
            foreach (var word in queryWords)
            {
                if (fullArticleText.Contains(word) || (word.Length >= 4 && fullArticleText.Contains(word.Substring(0, word.Length - 1))))
                {
                    matchedCount++;
                }
            }

            keywordScore = (double)matchedCount / queryWords.Count;
            double hybridScore = Math.Max(vecScore, keywordScore * 0.9);

            if (hybridScore >= 0.15)
            {
                scoredArticles.Add((article, hybridScore));
            }
        }

        scoredArticles = scoredArticles.OrderByDescending(x => x.Score).Take(topK).ToList();
        var topMatch = scoredArticles.FirstOrDefault();

        if (topMatch.Article == null || topMatch.Score < 0.15)
        {
            await LogInteractionAsync(tenantId, sessionId, query, topMatch.Score, false, cancellationToken);
            return new RagSearchResponseDto
            {
                Resolved = false,
                Answer = null,
                Sources = new List<RagSourceDto>(),
                TopScore = Math.Round(topMatch.Score, 4)
            };
        }

        var matchedArticle = topMatch.Article;
        var fallbackAnswer = $"{matchedArticle.Title}:\n{matchedArticle.Content}";

        await LogInteractionAsync(tenantId, sessionId, query, topMatch.Score, true, cancellationToken);

        return new RagSearchResponseDto
        {
            Resolved = true,
            Answer = fallbackAnswer,
            Sources = new List<RagSourceDto> { new RagSourceDto { ArticleId = matchedArticle.Id, Title = matchedArticle.Title } },
            TopScore = Math.Round(topMatch.Score, 4)
        };
    }

    private async Task LogInteractionAsync(
        Guid tenantId, 
        string sessionId, 
        string query, 
        double score, 
        bool resolved, 
        CancellationToken cancellationToken)
    {
        var log = new RagInteraction
        {
            TenantId = tenantId,
            SessionId = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString() : sessionId,
            Question = query,
            TopScore = Math.Round(score, 4),
            Resolved = resolved,
            TicketCreated = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RagInteractions.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static double ComputeCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA == null || vectorB == null || vectorA.Length != vectorB.Length || vectorA.Length == 0)
            return 0.0;

        double dotProduct = 0.0;
        double normA = 0.0;
        double normB = 0.0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        if (normA == 0.0 || normB == 0.0) return 0.0;

        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
}
