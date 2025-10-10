using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SoftFocusBackend.Therapy.Interfaces.REST.Hubs; // From NuGets

namespace SoftFocusBackend.Therapy.Infrastructure.ExternalServices
{
    public class SignalRChatService
    {
        private readonly IHubContext<ChatHub> _hubContext; // Assume ChatHub defined elsewhere

        public SignalRChatService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendMessageAsync(string receiverId, object message)
        {
            await _hubContext.Clients.User(receiverId).SendAsync("ReceiveMessage", message);
        }
    }
}