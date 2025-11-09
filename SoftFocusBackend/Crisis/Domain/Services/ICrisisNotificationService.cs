using SoftFocusBackend.Crisis.Domain.Model.Aggregates;

namespace SoftFocusBackend.Crisis.Domain.Services;

public interface ICrisisNotificationService
{
    Task NotifyPsychologistAsync(CrisisAlert alert);
}
