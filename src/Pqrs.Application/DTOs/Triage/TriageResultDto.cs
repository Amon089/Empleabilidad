using Pqrs.Domain.Enums;

namespace Pqrs.Application.DTOs.Triage;

public class TriageResultDto
{
    public TicketType Type { get; set; } = TicketType.PETITION;
    public Priority Priority { get; set; } = Priority.MEDIUM;
    public Sentiment Sentiment { get; set; } = Sentiment.NEUTRAL;
    public string Summary { get; set; } = string.Empty;
    public float? TypeConfidence { get; set; }
    public float? PriorityConfidence { get; set; }
    public float? SentimentConfidence { get; set; }
}
