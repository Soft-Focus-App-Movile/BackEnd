using MongoDB.Bson;
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
        // Buscar preferencia existente
        var preference = await _preferenceRepository.GetByUserAndTypeAsync(command.UserId, command.NotificationType);
        
        if (preference == null)
        {
            // Crear nueva preferencia
            preference = new NotificationPreference
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UserId = command.UserId,
                NotificationType = command.NotificationType,
                IsEnabled = command.IsEnabled,
                DeliveryMethod = command.DeliveryMethod ?? "push",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Convertir schedule si existe
            if (command.Schedule != null)
            {
                preference.Schedule = ConvertSchedule(command.Schedule);
            }

            await _preferenceRepository.CreateAsync(preference);
        }
        else
        {
            // Actualizar preferencia existente
            preference.IsEnabled = command.IsEnabled;
            
            if (!string.IsNullOrEmpty(command.DeliveryMethod))
                preference.DeliveryMethod = command.DeliveryMethod;
            
            if (command.Schedule != null)
            {
                preference.Schedule = ConvertSchedule(command.Schedule);
            }

            preference.UpdatedAt = DateTime.UtcNow;
            
            await _preferenceRepository.UpdateAsync(preference.Id, preference);
        }
        
        return preference;
    }

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
                existing.UpdatedAt = DateTime.UtcNow;
                await _preferenceRepository.UpdateAsync(existing.Id, existing);
                resultPreferences.Add(existing);
            }
            else
            {
                // Crear nueva preferencia
                defaultPref.Id = ObjectId.GenerateNewId().ToString();
                defaultPref.CreatedAt = DateTime.UtcNow;
                defaultPref.UpdatedAt = DateTime.UtcNow;
                await _preferenceRepository.CreateAsync(defaultPref);
                resultPreferences.Add(defaultPref);
            }
        }

        return resultPreferences;
    }

    // Helper: Convertir el schedule del request al formato del dominio
    private NotificationPreference.ScheduleSettings? ConvertSchedule(object? scheduleObj)
    {
        if (scheduleObj == null) return null;

        // Si viene como dynamic object del JSON
        if (scheduleObj is System.Text.Json.JsonElement jsonElement)
        {
            try
            {
                var startTime = jsonElement.GetProperty("start_time").GetString() ?? "09:00";
                var endTime = jsonElement.GetProperty("end_time").GetString() ?? "22:00";
                
                var daysOfWeek = new List<int>();
                if (jsonElement.TryGetProperty("days_of_week", out var daysArray))
                {
                    foreach (var day in daysArray.EnumerateArray())
                    {
                        daysOfWeek.Add(day.GetInt32());
                    }
                }

                return new NotificationPreference.ScheduleSettings
                {
                    QuietHours = new List<NotificationPreference.ScheduleSettings.QuietHourRange>
                    {
                        new NotificationPreference.ScheduleSettings.QuietHourRange
                        {
                            StartTime = startTime,
                            EndTime = endTime
                        }
                    },
                    ActiveDays = ConvertDaysOfWeekToActiveDays(daysOfWeek),
                    TimeZone = "UTC"
                };
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    // Helper: Convertir days_of_week (int) a active_days (string)
    private List<string> ConvertDaysOfWeekToActiveDays(List<int> daysOfWeek)
    {
        if (!daysOfWeek.Any())
            return new List<string> { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" };

        var daysMapping = new Dictionary<int, string>
        {
            { 1, "monday" },
            { 2, "tuesday" },
            { 3, "wednesday" },
            { 4, "thursday" },
            { 5, "friday" },
            { 6, "saturday" },
            { 7, "sunday" }
        };

        return daysOfWeek
            .Where(day => daysMapping.ContainsKey(day))
            .Select(day => daysMapping[day])
            .ToList();
    }

    private List<NotificationPreference> CreateDefaultPreferences(string userId)
    {
        return new List<NotificationPreference>
        {
            new NotificationPreference 
            { 
                UserId = userId, 
                NotificationType = "checkin-reminder",
                IsEnabled = true,
                DeliveryMethod = "push"
            },
            new NotificationPreference 
            { 
                UserId = userId, 
                NotificationType = "info",
                IsEnabled = true,
                DeliveryMethod = "push"
            },
            new NotificationPreference 
            { 
                UserId = userId, 
                NotificationType = "system-update",
                IsEnabled = true,
                DeliveryMethod = "push"
            }
        };
    }
}