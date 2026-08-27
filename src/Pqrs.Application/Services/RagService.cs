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
        if (string.IsNullOrWhiteSpace(query))
        {
            return new RagSearchResponseDto
            {
                Resolved = false,
                Answer = null,
                Sources = new List<RagSourceDto>(),
                TopScore = 0.0
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

        // 3. Compute vector similarity (Cosine Similarity)
        var scoredArticles = articles
            .Where(a => a.Embedding != null)
            .Select(a => new
            {
                Article = a,
                Score = ComputeCosineSimilarity(queryVec, a.Embedding!.ToArray())
            })
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();

        var topScore = scoredArticles.FirstOrDefault()?.Score ?? 0.0;

        // 4. Threshold check
        if (topScore < threshold || !scoredArticles.Any())
        {
            await LogInteractionAsync(tenantId, sessionId, query, topScore, false, cancellationToken);
            return new RagSearchResponseDto
            {
                Resolved = false,
                Answer = null,
                Sources = new List<RagSourceDto>(),
                TopScore = Math.Round(topScore, 4)
            };
        }

        // 5. Build context & generate answer via LLM
        var relevantArticles = scoredArticles.Select(x => x.Article).ToList();
        var answer = await _aiService.GenerateRagAnswerAsync(query, relevantArticles, cancellationToken);

        var sources = relevantArticles.Select(a => new RagSourceDto
        {
            ArticleId = a.Id,
            Title = a.Title
        }).ToList();

        await LogInteractionAsync(tenantId, sessionId, query, topScore, true, cancellationToken);

        return new RagSearchResponseDto
        {
            Resolved = true,
            Answer = answer,
            Sources = sources,
            TopScore = Math.Round(topScore, 4)
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
