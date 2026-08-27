using System;

namespace Pqrs.Application.DTOs.Widget;

public class RagSourceDto
{
    public Guid ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
}
