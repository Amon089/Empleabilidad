using System;
using Pqrs.Domain.Enums;

namespace Pqrs.Domain.Entities;

public class TicketStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid TicketId { get; set; }
    public TicketStatus? PreviousStatus { get; set; }
    public TicketStatus NewStatus { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;
}
