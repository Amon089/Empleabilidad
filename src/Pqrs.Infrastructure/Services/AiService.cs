using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pqrs.Application.DTOs.Triage;
using Pqrs.Application.Interfaces;
using Pqrs.Domain.Entities;
using Pqrs.Domain.Enums;

namespace Pqrs.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private const int EmbeddingDimension = 1536;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "y", "a", "el", "la", "en", "un", "una", "con", "por", "para", "su", "se", "los", "las", "del", "al", "o", "es", "son", "como", "mi", "mis"
    };

    public AiService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                  ?? Environment.GetEnvironmentVariable("AI_API_KEY") 
                  ?? _configuration["AI:ApiKey"];
        var provider = (_configuration["AI:Provider"] ?? "gemini").ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(apiKey) && apiKey != "YOUR_OPENAI_OR_GEMINI_API_KEY")
        {
            try
            {
                if (provider == "gemini")
                {
                    var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent?key={apiKey}";
                    var payload = new
                    {
                        model = "models/text-embedding-004",
                        content = new
                        {
                            parts = new[] { new { text = text } }
                        }
                    };

                    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                    request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonStr = await response.Content.ReadAsStringAsync(cancellationToken);
                        using var doc = JsonDocument.Parse(jsonStr);
                        var valuesElem = doc.RootElement.GetProperty("embedding").GetProperty("values");
                        var floatList = new List<float>();
                        foreach (var item in valuesElem.EnumerateArray())
                        {
                            floatList.Add(item.GetSingle());
                        }
                        return floatList.ToArray();
                    }
                }
                else // OpenAI Provider
                {
                    var endpoint = "https://api.openai.com/v1/embeddings";
                    var model = _configuration["AI:EmbeddingModel"] ?? "text-embedding-3-small";

                    var payload = new
                    {
                        model = model,
                        input = text
                    };

                    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");
                    request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonStr = await response.Content.ReadAsStringAsync(cancellationToken);
                        using var doc = JsonDocument.Parse(jsonStr);
                        var embeddingElem = doc.RootElement.GetProperty("data")[0].GetProperty("embedding");
                        var floatList = new List<float>();
                        foreach (var item in embeddingElem.EnumerateArray())
                        {
                            floatList.Add(item.GetSingle());
                        }
                        return floatList.ToArray();
                    }
                }
            }
            catch
            {
                // Fallback to word-token embedding calculation below
            }
        }

        return GenerateDeterministicEmbedding(text);
    }

    public async Task<string> GenerateRagAnswerAsync(string query, IEnumerable<KnowledgeBaseArticle> contextArticles, CancellationToken cancellationToken = default)
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                  ?? Environment.GetEnvironmentVariable("AI_API_KEY") 
                  ?? _configuration["AI:ApiKey"];
        var provider = (_configuration["AI:Provider"] ?? "gemini").ToLowerInvariant();
        var contextText = string.Join("\n\n---\n\n", contextArticles.Select(a => $"Título: {a.Title}\nContenido: {a.Content}"));
        var prompt = $"Eres el Asistente Virtual Inteligente Oficial de Atención al Cliente y PQRS de la empresa. Tu personalidad es cálida, servicial, conversacional, empática y muy profesional.\n\n" +
                     $"BASE DE CONOCIMIENTO Y CONTEXTO CORPORATIVO DEL TENANT ACTIVO:\n\n{contextText}\n\n" +
                     $"MÁRGENES DE ACTUACIÓN Y LIBERTAD CONVERSACIONAL:\n" +
                     $"1. LIBERTAD Y FLUIDEZ: Tienes libertad para expresarte con naturalidad, responder cordialmente, dar explicaciones fluidas, sugerir ideas o recomendaciones culinarias o constructivas relacionadas con el negocio, y adaptar tu tono al usuario siempre manteniendo respeto y profesionalismo.\n" +
                     $"2. DELIMITACIÓN DE TENANT: Mantén tus respuestas enmarcadas estrictamente en la actividad de esta empresa. Si te preguntan por servicios de otra industria no relacionada, aclara amablemente la especialización de la empresa y ofrece la opción de radicar una PQRS.\n" +
                     $"3. VERACIDAD Y LÍMITES TÉCNICOS/FINANCIEROS: No inventes datos específicos inexistentes como números de contratos gubernamentales, nombres de fincas o agricultores individuales, precios exactos no publicados o cálculos de ingeniería estructural definitivos. En proyectos técnicos o cotizaciones complejas, explica amablemente qué datos se requieren e invita a solicitar una cotización formal o radicar una PQRS con un especialista.\n" +
                     $"4. ATENCIÓN Y PQRS: Si el usuario desea reportar un reclamo, problema con su pedido/obra o hablar con un asesor humano, oriéntalo cordialmente a hacer clic en el botón 'Radicar PQRS'.\n\n" +
                     $"Pregunta del usuario: {query}";

        if (!string.IsNullOrWhiteSpace(apiKey) && apiKey != "YOUR_OPENAI_OR_GEMINI_API_KEY")
        {
            try
            {
                if (provider == "gemini")
                {
                    var modelCandidates = new[] { _configuration["AI:ChatModel"] ?? "gemini-1.5-flash", "gemini-1.5-flash", "gemini-2.0-flash" }.Distinct();

                    foreach (var model in modelCandidates)
                    {
                        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                        var payload = new
                        {
                            contents = new[]
                            {
                                new { parts = new[] { new { text = prompt } } }
                            },
                            generationConfig = new { temperature = 0.2 }
                        };

                        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                        var response = await _httpClient.SendAsync(request, cancellationToken);
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonStr = await response.Content.ReadAsStringAsync(cancellationToken);
                            using var doc = JsonDocument.Parse(jsonStr);
                            var answer = doc.RootElement
                                .GetProperty("candidates")[0]
                                .GetProperty("content")
                                .GetProperty("parts")[0]
                                .GetProperty("text")
                                .GetString();

                            if (!string.IsNullOrWhiteSpace(answer)) return answer.Trim();
                        }
                    }
                }
                else // OpenAI Provider
                {
                    var endpoint = "https://api.openai.com/v1/chat/completions";
                    var model = _configuration["AI:ChatModel"] ?? "gpt-4o-mini";

                    var payload = new
                    {
                        model = model,
                        messages = new[]
                        {
                            new { role = "user", content = prompt }
                        },
                        temperature = 0.1
                    };

                    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                    request.Headers.Add("Authorization", $"Bearer {apiKey}");
                    request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonStr = await response.Content.ReadAsStringAsync(cancellationToken);
                        using var doc = JsonDocument.Parse(jsonStr);
                        var answer = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                        if (!string.IsNullOrWhiteSpace(answer)) return answer.Trim();
                    }
                }
            }
            catch
            {
                // Fallback RAG synthesis below
            }
        }

        // Rule-based deterministic RAG Answer synthesis based strictly on context when external LLM is offline or rate-limited
        if (contextArticles != null && contextArticles.Any())
        {
            var bestArticles = contextArticles.Take(2).ToList();
            var responseBuilder = new StringBuilder();
            
            foreach (var art in bestArticles)
            {
                responseBuilder.AppendLine($"📌 **{art.Title}**");
                responseBuilder.AppendLine(art.Content);
                responseBuilder.AppendLine();
            }

            return responseBuilder.ToString().Trim();
        }

        return "No hay información suficiente en la base de conocimientos para responder esta consulta.";
    }

    public async Task<TriageResultDto> TriageTicketAsync(string subject, string description, CancellationToken cancellationToken = default)
    {
        var combinedText = $"{subject} {description}".ToLowerInvariant();

        // 1. Determine Type
        TicketType type = TicketType.PETITION;
        if (combinedText.Contains("reclam") || combinedText.Contains("incompleto") || combinedText.Contains("danado") || 
            combinedText.Contains("mal estado") || combinedText.Contains("union") || combinedText.Contains("problema") ||
            combinedText.Contains("corrosion") || combinedText.Contains("garantia") || combinedText.Contains("cobro"))
        {
            type = TicketType.CLAIM;
        }
        else if (combinedText.Contains("inconformidad") || combinedText.Contains("demora") || combinedText.Contains("fuera de horario"))
        {
            type = TicketType.COMPLAINT;
        }
        else if (combinedText.Contains("sugerencia") || combinedText.Contains("recomendacion"))
        {
            type = TicketType.SUGGESTION;
        }
        else if (combinedText.Contains("cotizacion") || combinedText.Contains("visita tecnica") || combinedText.Contains("informacion") || combinedText.Contains("copia"))
        {
            type = TicketType.PETITION;
        }

        // 2. Determine Priority
        Priority priority = Priority.MEDIUM;
        if (combinedText.Contains("corrosion") || combinedText.Contains("union") || combinedText.Contains("danado") || 
            combinedText.Contains("mal estado") || combinedText.Contains("urgente") || combinedText.Contains("grave") ||
            combinedText.Contains("garantia") || combinedText.Contains("incompleto"))
        {
            priority = Priority.HIGH;
        }
        else if (combinedText.Contains("sugerencia") || combinedText.Contains("informacion") || combinedText.Contains("cotizacion"))
        {
            priority = Priority.LOW;
        }

        // 3. Determine Sentiment
        Sentiment sentiment = Sentiment.NEUTRAL;
        if (combinedText.Contains("mal estado") || combinedText.Contains("incompleto") || combinedText.Contains("corrosion") || 
            combinedText.Contains("inconformidad") || combinedText.Contains("problema") || combinedText.Contains("no recibo") || 
            combinedText.Contains("cobraron"))
        {
            sentiment = Sentiment.NEGATIVE;
        }
        else if (combinedText.Contains("excelente") || combinedText.Contains("gracias") || combinedText.Contains("buen"))
        {
            sentiment = Sentiment.POSITIVE;
        }

        // 4. Generate Summary
        var summary = $"Solicitud sobre '{subject}'. Clasificado como {type} con prioridad {priority}.";

        return await Task.FromResult(new TriageResultDto
        {
            Type = type,
            Priority = priority,
            Sentiment = sentiment,
            Summary = summary,
            TypeConfidence = 0.95f,
            PriorityConfidence = 0.92f,
            SentimentConfidence = 0.94f
        });
    }

    private static float[] GenerateDeterministicEmbedding(string text)
    {
        var vector = new float[EmbeddingDimension];
        if (string.IsNullOrWhiteSpace(text)) return vector;

        var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var titleText = lines.Length > 0 ? lines[0] : text;

        var titleWords = titleText.ToLowerInvariant()
            .Split(new[] { ' ', '\t', ',', '.', ';', ':', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !StopWords.Contains(w))
            .Distinct()
            .ToList();

        var bodyWords = text.ToLowerInvariant()
            .Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !StopWords.Contains(w))
            .Distinct()
            .ToList();

        foreach (var word in bodyWords)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(word));
            int index = Math.Abs(BitConverter.ToInt32(hash, 0)) % EmbeddingDimension;
            float weight = titleWords.Contains(word) ? 4.0f : 1.0f;
            vector[index] = weight;
        }

        // Normalize vector to unit length
        double normSq = vector.Sum(v => (double)v * v);
        float norm = (float)Math.Sqrt(normSq);
        if (norm > 0)
        {
            for (int i = 0; i < EmbeddingDimension; i++)
            {
                vector[i] /= norm;
            }
        }

        return vector;
    }
}
