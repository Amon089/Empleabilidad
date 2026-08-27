using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Pqrs.API.Hubs;

public class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantIdClaim = Context.User?.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrWhiteSpace(tenantIdClaim))
        {
            tenantIdClaim = Context.GetHttpContext()?.Request.Query["tenant_id"].ToString();
        }

        if (Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant_{tenantId}");
        }

        await base.OnConnectedAsync();
    }
}
