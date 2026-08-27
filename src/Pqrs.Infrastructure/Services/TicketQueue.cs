using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Pqrs.Application.Interfaces;

namespace Pqrs.Infrastructure.Services;

public class TicketQueue : ITicketQueue
{
    private readonly Channel<Guid> _channel;

    public TicketQueue()
    {
        var options = new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateUnbounded<Guid>(options);
    }

    public async ValueTask EnqueueTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(ticketId, cancellationToken);
    }

    public async ValueTask<Guid> DequeueTicketAsync(CancellationToken cancellationToken = default)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}
