using System;
using System.Collections.Generic;
using Pqrs.Domain.Enums;

namespace Pqrs.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketType Type { get; set; } = TicketType.PETITION;
    public TicketStatus Status { get; set; } = TicketStatus.TRIAGE_PENDING;
    public Priority Priority { get; set; } = Priority.MEDIUM;
    public Sentiment Sentiment { get; set; } = Sentiment.NEUTRAL;
    public string Summary { get; set; } = string.Empty;
    public bool ResolvedByRag { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<TicketStatusHistory> StatusHistory { get; set; } = new List<TicketStatusHistory>();
}
