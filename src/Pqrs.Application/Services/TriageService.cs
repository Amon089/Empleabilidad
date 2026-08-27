using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pqrs.Application.Interfaces;
using Pqrs.Domain.Entities;
using Pqrs.Domain.Enums;

namespace Pqrs.Application.Services;

public class TriageService
{
    private readonly IApplicationDbContext _context;
    private readonly IAiService _aiService;
    private readonly INotificationService _notificationService;

    public TriageService(
        IApplicationDbContext context, 
        IAiService aiService, 
        INotificationService notificationService)
    {
        _context = context;
        _aiService = aiService;
        _notificationService = notificationService;
    }

    public async Task ProcessTicketTriageAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket == null) return;

        var triageResult = await _aiService.TriageTicketAsync(ticket.Subject, ticket.Description, cancellationToken);

        ticket.Type = triageResult.Type;
        ticket.Priority = triageResult.Priority;
        ticket.Sentiment = triageResult.Sentiment;
        ticket.Summary = triageResult.Summary;
        ticket.Status = TicketStatus.PENDING;
        ticket.UpdatedAt = DateTime.UtcNow;

        _context.TicketStatusHistories.Add(new TicketStatusHistory
        {
            TenantId = ticket.TenantId,
            TicketId = ticket.Id,
            PreviousStatus = TicketStatus.TRIAGE_PENDING,
            NewStatus = TicketStatus.PENDING,
            ChangedBy = "AI_TRIAGE_SERVICE",
            Reason = $"AI Triage completed. Summary: {triageResult.Summary}",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        if (ticket.Priority == Priority.HIGH || ticket.Sentiment == Sentiment.NEGATIVE)
        {
            await _notificationService.NotifyCriticalTicketAsync(ticket.TenantId, ticket, cancellationToken);
        }
    }
}
