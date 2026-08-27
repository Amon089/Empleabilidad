using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pqrs.Application.DTOs.Ticket;
using Pqrs.Application.DTOs.Widget;
using Pqrs.Application.Services;
using Pqrs.Domain.Enums;
using Pqrs.Infrastructure;
using Pqrs.Infrastructure.Persistence;
using Pqrs.Infrastructure.Services;
using Xunit;

namespace Pqrs.UnitTests;

public class TicketServiceTests
{
    private PqrsDbContext GetInMemoryDbContext(string dbName, TenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<PqrsDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new PqrsDbContext(options, tenantContext);
    }

    [Fact]
    public async Task CreateWidgetTicketAsync_CreatesTicketWithPendingTriageStatus()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TenantContext();
        tenantContext.SetTenantId(tenantId);

        using var dbContext = GetInMemoryDbContext(Guid.NewGuid().ToString(), tenantContext);
        var queue = new TicketQueue();
        var ticketService = new TicketService(dbContext, queue, tenantContext);

        var dto = new CreateTicketWidgetDto
        {
            CustomerName = "Juan Perez",
            CustomerEmail = "juan@example.com",
            Subject = "Pedido incompleto",
            Description = "Mi pedido llego incompleto."
        };

        var created = await ticketService.CreateWidgetTicketAsync(dto, tenantId);

        Assert.NotNull(created);
        Assert.Equal(TicketStatus.TRIAGE_PENDING, created.Status);
        Assert.Equal("Juan Perez", created.CustomerName);
    }

    [Fact]
    public async Task UpdateTicketStatusAsync_UpdatesStatusAndAddsHistory()
    {
        var tenantId = Guid.NewGuid();
        var tenantContext = new TenantContext();
        tenantContext.SetTenantId(tenantId);

        using var dbContext = GetInMemoryDbContext(Guid.NewGuid().ToString(), tenantContext);
        var queue = new TicketQueue();
        var ticketService = new TicketService(dbContext, queue, tenantContext);

        var dto = new CreateTicketWidgetDto
        {
            CustomerName = "Juan Perez",
            CustomerEmail = "juan@example.com",
            Subject = "Pedido incompleto",
            Description = "Mi pedido llego incompleto."
        };

        var created = await ticketService.CreateWidgetTicketAsync(dto, tenantId);

        var updateDto = new UpdateTicketStatusDto
        {
            Status = TicketStatus.RESOLVED,
            Priority = Priority.HIGH,
            Reason = "Issue resolved by customer support"
        };

        var updated = await ticketService.UpdateTicketStatusAsync(created.Id, updateDto, "agent@example.com");

        Assert.Equal(TicketStatus.RESOLVED, updated.Status);
        Assert.Equal(Priority.HIGH, updated.Priority);
        Assert.NotEmpty(updated.StatusHistory);
        Assert.Equal("agent@example.com", updated.StatusHistory[0].ChangedBy);
    }
}
