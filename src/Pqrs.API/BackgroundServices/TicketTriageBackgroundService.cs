using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pqrs.Application.Interfaces;
using Pqrs.Application.Services;

namespace Pqrs.API.BackgroundServices;

public class TicketTriageBackgroundService : BackgroundService
{
    private readonly ITicketQueue _ticketQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TicketTriageBackgroundService> _logger;

    public TicketTriageBackgroundService(
        ITicketQueue ticketQueue, 
        IServiceProvider serviceProvider, 
        ILogger<TicketTriageBackgroundService> logger)
    {
        _ticketQueue = ticketQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TicketTriageBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var ticketId = await _ticketQueue.DequeueTicketAsync(stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var triageService = scope.ServiceProvider.GetRequiredService<TriageService>();

                _logger.LogInformation("Starting AI Triage for ticket {TicketId}", ticketId);
                await triageService.ProcessTicketTriageAsync(ticketId, stoppingToken);
                _logger.LogInformation("Completed AI Triage for ticket {TicketId}", ticketId);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI Triage for background ticket queue.");
            }
        }
    }
}
