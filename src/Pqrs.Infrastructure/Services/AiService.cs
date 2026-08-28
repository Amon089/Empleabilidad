using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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

        var prompt = $"Eres el Asistente Virtual Oficial de la empresa. Tu función es responder la consulta del cliente utilizando ÚNICAMENTE la base de conocimiento autorizada provista a continuación.\n\n" +
                     $"BASE DE CONOCIMIENTO AUTORIZADA:\n\n{contextText}\n\n" +
                     $"INSTRUCCIONES DE FORMATO Y RESPUESTA:\n" +
                     $"1. Responde de forma amable, cercana, profesional y natural en español.\n" +
                     $"2. Sintetiza la respuesta directamente al cliente usando viñetas o párrafos breves.\n" +
                     $"3. NUNCA incluyas identificadores internos como 'P101:', 'R101:', 'P151:', ni títulos de archivos de entrenamiento como 'PREGUNTAS Y RESPUESTAS DE...'.\n" +
                     $"4. RESPONDE ÚNICAMENTE SI EL CONTEXTO ARRIBA ES SUFICIENTE Y PERTINENTE. Si no existe información suficiente, responde exactamente: 'No encuentro información suficiente para responder esta consulta.'\n" +
                     $"5. NUNCA inventes precios, horarios, fechas, disponibilidad, contratos o especificaciones no presentes en el contexto.\n\n" +
                     $"Pregunta del usuario: {query}";

        if (!string.IsNullOrWhiteSpace(apiKey) && apiKey != "YOUR_OPENAI_OR_GEMINI_API_KEY")
        {
            try
            {
                if (provider == "gemini")
                {
                    var modelCandidates = new[] { _configuration["AI:ChatModel"] ?? "gemini-3.5-flash", "gemini-3.5-flash", "gemini-flash-latest", "gemini-2.5-flash" }.Distinct();

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

                            if (!string.IsNullOrWhiteSpace(answer)) return CleanFormattedAnswer(answer);
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
                        temperature = 0.2
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
                        if (!string.IsNullOrWhiteSpace(answer)) return CleanFormattedAnswer(answer);
                    }
                }
            }
            catch
            {
                // Fallback RAG synthesis below
            }
        }

        // Clean Fallback RAG Answer synthesis when external LLM is offline or rate-limited
        if (contextArticles != null && contextArticles.Any())
        {
            return SynthesizeCleanFallback(query, contextArticles);
        }

        return "No puedo procesar esta consulta en este momento. Si necesitas atención, puedes registrar una PQRS.";
    }

    private string SynthesizeCleanFallback(string query, IEnumerable<KnowledgeBaseArticle> contextArticles)
    {
        var topArticle = contextArticles.FirstOrDefault();
        if (topArticle == null || string.IsNullOrWhiteSpace(topArticle.Content))
        {
            return "No encuentro información suficiente para responder esta consulta.";
        }

        var lines = topArticle.Content.Split('\n');
        var cleanLines = new List<string>();

        foreach (var line in lines)
        {
            var l = line.Trim();
            if (l.StartsWith("PREGUNTAS Y RESPUESTAS", StringComparison.OrdinalIgnoreCase) ||
                l.Contains("Q&As):", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Extract answers (R1:, R2:, R101:)
            if (l.StartsWith("R") && Regex.IsMatch(l, @"^R\d+:"))
            {
                var cleanAnswerText = Regex.Replace(l, @"^R\d+:\s*", "");
                cleanLines.Add("• " + cleanAnswerText);
            }
            else if (!l.StartsWith("P") || !Regex.IsMatch(l, @"^P\d+:"))
            {
                var cleanLineText = Regex.Replace(l, @"^[PR]\d+:\s*", "");
                if (!string.IsNullOrWhiteSpace(cleanLineText))
                {
                    cleanLines.Add(cleanLineText);
                }
            }
        }

        if (cleanLines.Any())
        {
            return string.Join("\n", cleanLines.Take(8)).Trim();
        }

        return topArticle.Content.Trim();
    }

    private string CleanFormattedAnswer(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var cleaned = Regex.Replace(text, @"^[PR]\d+:\s*", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"^P\d+\s+a\s+P\d+:\s*", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"PREGUNTAS Y RESPUESTAS DE.*?\n", "", RegexOptions.IgnoreCase);

        return cleaned.Trim();
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
        else if (combinedText.Contains("sugerencia") || combinedText.Contains("felicitaciones") || combinedText.Contains("consulta"))
        {
            priority = Priority.LOW;
        }

        // 3. Determine Sentiment
        Sentiment sentiment = Sentiment.NEUTRAL;
        if (combinedText.Contains("excelente") || combinedText.Contains("felicitaciones") || combinedText.Contains("buen servicio") || combinedText.Contains("gracias"))
        {
            sentiment = Sentiment.POSITIVE;
        }
        else if (combinedText.Contains("inconform") || combinedText.Contains("pesimo") || combinedText.Contains("mal estado") || combinedText.Contains("retraso") || combinedText.Contains("queja") || combinedText.Contains("danado"))
        {
            sentiment = Sentiment.NEGATIVE;
        }

        var summary = $"Solicitud de tipo {type} clasificada con prioridad {priority} y sentimiento {sentiment}.";

        return await Task.FromResult(new TriageResultDto
        {
            Type = type,
            Priority = priority,
            Sentiment = sentiment,
            Summary = summary
        });
    }

    private float[] GenerateDeterministicEmbedding(string text)
    {
        var tokens = text.ToLowerInvariant()
            .Split(new[] { ' ', '.', ',', ';', ':', '!', '?', '-', '_', '(', ')', '[', ']', '"', '\'', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => !StopWords.Contains(t) && t.Length > 1)
            .Distinct()
            .ToList();

        var vector = new float[EmbeddingDimension];
        if (!tokens.Any())
        {
            vector[0] = 1.0f;
            return vector;
        }

        foreach (var token in tokens)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));

            for (int i = 0; i < 16; i++)
            {
                int index = BitConverter.ToUInt16(hash, i * 2) % EmbeddingDimension;
                vector[index] += 1.0f;
            }
        }

        double norm = 0;
        for (int i = 0; i < EmbeddingDimension; i++)
        {
            norm += vector[i] * vector[i];
        }
        norm = Math.Sqrt(norm);

        if (norm > 0)
        {
            for (int i = 0; i < EmbeddingDimension; i++)
            {
                vector[i] = (float)(vector[i] / norm);
            }
        }

        return vector;
    }
}
