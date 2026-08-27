using Pqrs.Domain.Enums;

namespace Pqrs.Application.DTOs.Ticket;

public class TicketFilterDto
{
    public TicketStatus? Status { get; set; }
    public Priority? Priority { get; set; }
    public TicketType? Type { get; set; }
    public Sentiment? Sentiment { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
