using System.Collections.Generic;

namespace Pqrs.Application.DTOs.Widget;

public class RagSearchResponseDto
{
    public bool Resolved { get; set; }
    public string? Answer { get; set; }
    public List<RagSourceDto> Sources { get; set; } = new();
    public double TopScore { get; set; }
}
