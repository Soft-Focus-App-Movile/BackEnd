using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Services
{
    public class ChatModerationService : IChatModerationService
    {
        public async Task<MessageContent> ModerateContentAsync(MessageContent content)
        {
            // Basic moderation - for now just return the content as is
            // In the future, you can add:
            // - Profanity filtering
            // - Spam detection
            // - Inappropriate content detection
            // - Crisis keywords detection

            await Task.CompletedTask; // Placeholder for async operation
            return content;
        }
    }
}
