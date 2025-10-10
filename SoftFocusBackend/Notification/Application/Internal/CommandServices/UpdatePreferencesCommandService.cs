using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Notification.Domain.Model.Aggregates;
using SoftFocusBackend.Notification.Domain.Repositories;

namespace SoftFocusBackend.Notification.Application.Internal.CommandServices;

public class UpdatePreferencesCommandService
{
    private readonly INotificationPreferenceRepository _preferenceRepository;

    public UpdatePreferencesCommandService(INotificationPreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository;
    }

    public async Task<NotificationPreference> HandleAsync(UpdatePreferencesCommand command)
    {
        var preference = await _preferenceRepository.GetByUserAndTypeAsync(command.UserId, command.NotificationType);
        
        if (preference == null)
        {
            preference = new NotificationPreference
            {
                UserId = command.UserId,
                NotificationType = command.NotificationType
            };
        }
        
        preference.IsEnabled = command.IsEnabled;
        
        if (!string.IsNullOrEmpty(command.DeliveryMethod))
            preference.DeliveryMethod = command.DeliveryMethod;
        
        if (command.Schedule != null)
        {
            // Convert schedule object to ScheduleSettings
            // Implementation depends on the actual schedule format
        }
        
        if (string.IsNullOrEmpty(preference.Id))
            await _preferenceRepository.CreateAsync(preference);
        else
            await _preferenceRepository.UpdateAsync(preference.Id, preference);
        
        return preference;
    }
}