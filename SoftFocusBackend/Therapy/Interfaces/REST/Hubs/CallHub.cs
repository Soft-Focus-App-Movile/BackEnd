using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SoftFocusBackend.Therapy.Interfaces.REST.Hubs
{
    /// <summary>
    /// Real-time signaling channel for calls. The server pushes these events to clients:
    ///   - "IncomingCall"  → an invitee is being rung.
    ///   - "CallAccepted"  → an invitee accepted (sent to the caller).
    ///   - "CallRejected"  → an invitee rejected (sent to the caller).
    ///   - "CallEnded"     → the call ended/was cancelled (sent to all other participants).
    /// Clients connect with their JWT; SignalR maps connections to user_id via NameIdentifierUserIdProvider.
    /// Media itself flows through the Agora SDK, not through this hub.
    /// </summary>
    [Authorize]
    public class CallHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
