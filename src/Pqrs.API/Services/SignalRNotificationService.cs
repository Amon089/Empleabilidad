using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Pqrs.API.Hubs;
using Pqrs.Application.Interfaces;
using Pqrs.Domain.Entities;

namespace Pqrs.API.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationsHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyCriticalTicketAsync(Guid tenantId, Ticket ticket, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            ticketId = ticket.Id,
            priority = ticket.Priority.ToString(),
            sentiment = ticket.Sentiment.ToString(),
            summary = ticket.Summary
        };

        await _hubContext.Clients
            .Group($"tenant_{tenantId}")
            .SendAsync("ticket.critical", payload, cancellationToken);
    }
}
