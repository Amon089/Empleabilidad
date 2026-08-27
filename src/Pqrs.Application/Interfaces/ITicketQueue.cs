using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pqrs.Application.Interfaces;

public interface ITicketQueue
{
    ValueTask EnqueueTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
    ValueTask<Guid> DequeueTicketAsync(CancellationToken cancellationToken = default);
}
