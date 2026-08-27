namespace Pqrs.Application.DTOs.KnowledgeBase;

public class UpdateArticleDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool? IsActive { get; set; }
}
