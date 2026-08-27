using System;
using System.Threading;
using System.Threading.Tasks;
using Pqrs.Domain.Entities;

namespace Pqrs.Application.Interfaces;

public interface INotificationService
{
    Task NotifyCriticalTicketAsync(Guid tenantId, Ticket ticket, CancellationToken cancellationToken = default);
}
