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
            await _preferenceRepository.CreateAsync(preference); // Esto retorna void
        else
            await _preferenceRepository.UpdateAsync(preference.Id, preference); // Esto también retorna void
        
        return preference;
    }

    // 🆕 MÉTODO CORREGIDO PARA RESET
    public async Task<IEnumerable<NotificationPreference>> HandleAsync(ResetPreferencesCommand command)
    {
        // 1. Obtener preferencias actuales
        var currentPreferences = await _preferenceRepository.GetByUserIdAsync(command.UserId);
        
        // 2. Crear preferencias por defecto
        var defaultPreferences = CreateDefaultPreferences(command.UserId);

        // 3. Actualizar o crear preferencias
        var resultPreferences = new List<NotificationPreference>();
        
        foreach (var defaultPref in defaultPreferences)
        {
            var existing = currentPreferences.FirstOrDefault(p => p.NotificationType == defaultPref.NotificationType);
            
            if (existing != null)
            {
                // Actualizar preferencia existente
                existing.IsEnabled = defaultPref.IsEnabled;
                existing.DeliveryMethod = defaultPref.DeliveryMethod;
                await _preferenceRepository.UpdateAsync(existing.Id, existing);
                resultPreferences.Add(existing);
            }
            else
            {
                // Crear nueva preferencia : no asignamos el void result
                var newPreference = new NotificationPreference
                {
                    UserId = defaultPref.UserId,
                    NotificationType = defaultPref.NotificationType,
                    IsEnabled = defaultPref.IsEnabled,
                    DeliveryMethod = defaultPref.DeliveryMethod
                };
                
                await _preferenceRepository.CreateAsync(newPreference);
                resultPreferences.Add(newPreference);
            }
        }

        return resultPreferences;
    }

    private List<NotificationPreference> CreateDefaultPreferences(string userId)
    {
        return new List<NotificationPreference>
        {
            new NotificationPreference 
            { 
                UserId = userId, 
                NotificationType = "Reminder",
                IsEnabled = true,
                DeliveryMethod = "Push"
            },
            new NotificationPreference 
            { 
                UserId = userId, 
                NotificationType = "Therapy",
                IsEnabled = true,
                DeliveryMethod = "Push"
            },
            new NotificationPreference 
            { 
                UserId = userId, 
                NotificationType = "Crisis",
                IsEnabled = true,
                DeliveryMethod = "Push"
            },
            new NotificationPreference 
            { 
                UserId = userId, 
                NotificationType = "Content",
                IsEnabled = true,
                DeliveryMethod = "Push"
            },
            new NotificationPreference 
            { 
                UserId = userId, 
                NotificationType = "System",
                IsEnabled = true,
                DeliveryMethod = "Push"
            }
        };
    }
}