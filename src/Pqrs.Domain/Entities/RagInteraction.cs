using System;

namespace Pqrs.Domain.Entities;

public class RagInteraction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public double TopScore { get; set; }
    public bool Resolved { get; set; }
    public bool TicketCreated { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
