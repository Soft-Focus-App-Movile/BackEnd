using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SoftFocusBackend.Therapy.Infrastructure.ExternalServices
{
    // Esta clase le dice a SignalR que use el "NameIdentifier" (que es tu user_id)
    // como el identificador de usuario para Clients.User()
    public class NameIdentifierUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}