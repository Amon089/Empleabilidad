using System;
using System.Collections.Generic;
using Pqrs.Domain.Enums;

namespace Pqrs.Application.DTOs.Ticket;

public class TicketDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public TicketStatus Status { get; set; }
    public Priority Priority { get; set; }
    public Sentiment Sentiment { get; set; }
    public string Summary { get; set; } = string.Empty;
    public bool ResolvedByRag { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<TicketStatusHistoryDto> StatusHistory { get; set; } = new();
}
