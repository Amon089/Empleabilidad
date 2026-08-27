using System;
using Pqrs.Domain.Enums;

namespace Pqrs.Application.DTOs.Ticket;

public class TicketStatusHistoryDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public TicketStatus? PreviousStatus { get; set; }
    public TicketStatus NewStatus { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
