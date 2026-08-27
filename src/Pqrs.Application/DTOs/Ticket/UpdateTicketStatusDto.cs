using Pqrs.Domain.Enums;

namespace Pqrs.Application.DTOs.Ticket;

public class UpdateTicketStatusDto
{
    public TicketStatus Status { get; set; }
    public Priority? Priority { get; set; }
    public string? Reason { get; set; }
}
