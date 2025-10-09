using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Services
{
    public interface IChatModerationService
    {
        Task<MessageContent> ModerateContentAsync(MessageContent content);
    }
}