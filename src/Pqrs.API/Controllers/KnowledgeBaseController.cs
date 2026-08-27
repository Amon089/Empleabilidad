using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pqrs.Application.DTOs.KnowledgeBase;
using Pqrs.Application.Services;

namespace Pqrs.API.Controllers;

[ApiController]
[Route("api/v1/kb-articles")]
[Authorize]
public class KnowledgeBaseController : ControllerBase
{
    private readonly KnowledgeBaseService _kbService;

    public KnowledgeBaseController(KnowledgeBaseService kbService)
    {
        _kbService = kbService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ArticleDto>>> GetArticles(CancellationToken cancellationToken)
    {
        var articles = await _kbService.GetArticlesAsync(cancellationToken);
        return Ok(articles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ArticleDto>> GetArticleById(Guid id, CancellationToken cancellationToken)
    {
        var article = await _kbService.GetArticleByIdAsync(id, cancellationToken);
        return Ok(article);
    }

    [HttpPost]
    public async Task<ActionResult<ArticleDto>> CreateArticle([FromBody] CreateArticleDto dto, CancellationToken cancellationToken)
    {
        var article = await _kbService.CreateArticleAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetArticleById), new { id = article.Id }, article);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ArticleDto>> UpdateArticle(Guid id, [FromBody] UpdateArticleDto dto, CancellationToken cancellationToken)
    {
        var article = await _kbService.UpdateArticleAsync(id, dto, cancellationToken);
        return Ok(article);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteArticle(Guid id, CancellationToken cancellationToken)
    {
        await _kbService.DeleteArticleAsync(id, cancellationToken);
        return NoContent();
    }
}
