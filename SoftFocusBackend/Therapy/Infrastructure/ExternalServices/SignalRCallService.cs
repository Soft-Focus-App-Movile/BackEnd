using Microsoft.AspNetCore.SignalR;
using SoftFocusBackend.Therapy.Interfaces.REST.Hubs;

namespace SoftFocusBackend.Therapy.Infrastructure.ExternalServices
{
    /// <summary>
    /// Pushes call signaling events to specific users over the <see cref="CallHub"/>.
    /// Mirrors <see cref="SignalRChatService"/> used by the chat feature.
    /// </summary>
    public class SignalRCallService
    {
        private readonly IHubContext<CallHub> _hubContext;

        public SignalRCallService(IHubContext<CallHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyIncomingCallAsync(string userId, object payload) =>
            _hubContext.Clients.User(userId).SendAsync("IncomingCall", payload);

        public Task NotifyCallAcceptedAsync(string userId, object payload) =>
            _hubContext.Clients.User(userId).SendAsync("CallAccepted", payload);

        public Task NotifyCallRejectedAsync(string userId, object payload) =>
            _hubContext.Clients.User(userId).SendAsync("CallRejected", payload);

        public Task NotifyCallEndedAsync(string userId, object payload) =>
            _hubContext.Clients.User(userId).SendAsync("CallEnded", payload);
    }
}
