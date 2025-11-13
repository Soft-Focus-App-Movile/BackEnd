using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SoftFocusBackend.Therapy.Interfaces.REST.Hubs
{
    [Authorize] // Ensures only authenticated users can connect
    public class ChatHub : Hub
    {
        // Method to send a message to a specific user
        public async Task SendMessage(string receiverId, string message)
        {
            // Envía el mensaje a todas las conexiones asociadas con ese receiverId (UserId)
            await Clients.User(receiverId).SendAsync("ReceiveMessage", message);
        }

        // Method to broadcast a message to all connected clients in the relationship
        // This could be extended based on relationshipId if needed
        public async Task BroadcastMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        // Override OnConnectedAsync to track user connections
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier; // Assumes user ID is set in claims
            if (userId != null)
            {
                // Store user connection (e.g., in memory or database for scalability)
                // This is a simple in-memory approach; use a service for production
                // Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        // Override OnDisconnectedAsync to clean up
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            if (userId != null)
            {
                // Remove user connection
                // Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Helper method to get connection ID by user ID (simplified)
        private string GetConnectionIdByUserId(string userId)
        {
            // In a real app, this would query a user-connection mapping (e.g., from a service or database)
            // For now, this is a placeholder; implement a proper mapping mechanism
            // Example: return _connections.FirstOrDefault(c => c.Value == userId).Key;
            return null; // Replace with actual logic
        }
    }
}