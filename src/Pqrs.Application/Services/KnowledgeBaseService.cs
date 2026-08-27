using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pqrs.Application.DTOs.KnowledgeBase;
using Pqrs.Application.Exceptions;
using Pqrs.Application.Interfaces;
using Pqrs.Domain.Entities;
using Pqrs.Domain.Interfaces;

namespace Pqrs.Application.Services;

public class KnowledgeBaseService
{
    private readonly IApplicationDbContext _context;
    private readonly IAiService _aiService;
    private readonly ITenantContext _tenantContext;

    public KnowledgeBaseService(
        IApplicationDbContext context, 
        IAiService aiService, 
        ITenantContext tenantContext)
    {
        _context = context;
        _aiService = aiService;
        _tenantContext = tenantContext;
    }

    public async Task<List<ArticleDto>> GetArticlesAsync(CancellationToken cancellationToken = default)
    {
        var articles = await _context.KnowledgeBaseArticles
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return articles.Select(MapToDto).ToList();
    }

    public async Task<ArticleDto> GetArticleByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var article = await _context.KnowledgeBaseArticles
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (article == null)
        {
            throw new NotFoundException("KnowledgeBaseArticle", id);
        }

        return MapToDto(article);
    }

    public async Task<ArticleDto> CreateArticleAsync(CreateArticleDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
        {
            throw new ValidationException("Title and Content are required.");
        }

        var embeddingArr = await _aiService.GenerateEmbeddingAsync($"{dto.Title}\n{dto.Content}", cancellationToken);

        var article = new KnowledgeBaseArticle
        {
            TenantId = _tenantContext.TenantId,
            Title = dto.Title.Trim(),
            Content = dto.Content.Trim(),
            Embedding = new Vector(embeddingArr),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.KnowledgeBaseArticles.Add(article);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(article);
    }

    public async Task<ArticleDto> UpdateArticleAsync(Guid id, UpdateArticleDto dto, CancellationToken cancellationToken = default)
    {
        var article = await _context.KnowledgeBaseArticles
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (article == null)
        {
            throw new NotFoundException("KnowledgeBaseArticle", id);
        }

        bool contentChanged = false;

        if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != article.Title)
        {
            article.Title = dto.Title.Trim();
            contentChanged = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Content) && dto.Content != article.Content)
        {
            article.Content = dto.Content.Trim();
            contentChanged = true;
        }

        if (dto.IsActive.HasValue)
        {
            article.IsActive = dto.IsActive.Value;
        }

        if (contentChanged)
        {
            var embeddingArr = await _aiService.GenerateEmbeddingAsync($"{article.Title}\n{article.Content}", cancellationToken);
            article.Embedding = new Vector(embeddingArr);
        }

        article.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(article);
    }

    public async Task DeleteArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var article = await _context.KnowledgeBaseArticles
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (article == null)
        {
            throw new NotFoundException("KnowledgeBaseArticle", id);
        }

        _context.KnowledgeBaseArticles.Remove(article);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static ArticleDto MapToDto(KnowledgeBaseArticle article)
    {
        return new ArticleDto
        {
            Id = article.Id,
            TenantId = article.TenantId,
            Title = article.Title,
            Content = article.Content,
            IsActive = article.IsActive,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt
        };
    }
}
