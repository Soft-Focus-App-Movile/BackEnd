using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SoftFocusBackend.Crisis.Interfaces.Hubs;

[Authorize]
public class CrisisHub : Hub
{
    public async Task SubscribeToCrisisAlerts(string psychologistId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"psychologist-{psychologistId}");
    }

    public async Task UnsubscribeFromCrisisAlerts(string psychologistId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"psychologist-{psychologistId}");
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
