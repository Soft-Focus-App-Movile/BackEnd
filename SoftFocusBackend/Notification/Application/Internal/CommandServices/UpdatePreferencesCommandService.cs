using MongoDB.Bson;
using System.Text.Json;
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

            // 🔥 FIX: Convertir schedule si existe
            if (command.Schedule != null)
            {
                var scheduleSettings = ConvertSchedule(command.Schedule);
                if (scheduleSettings != null)
                {
                    preference.Schedule = scheduleSettings;
                }
            }

            await _preferenceRepository.CreateAsync(preference);
        }
        else
        {
            // Actualizar preferencia existente
            preference.IsEnabled = command.IsEnabled;
            
            if (!string.IsNullOrEmpty(command.DeliveryMethod))
                preference.DeliveryMethod = command.DeliveryMethod;
            
            // 🔥 FIX: Actualizar schedule si existe
            if (command.Schedule != null)
            {
                var scheduleSettings = ConvertSchedule(command.Schedule);
                if (scheduleSettings != null)
                {
                    preference.Schedule = scheduleSettings;
                }
            }
            else
            {
                // Si el schedule es null explícitamente, mantener el existente
                // Solo eliminarlo si se envía null intencionalmente
            }

            preference.UpdatedAt = DateTime.UtcNow;
            
            await _preferenceRepository.UpdateAsync(preference.Id, preference);
        }
        
        return preference;
    }

    public async Task<IEnumerable<NotificationPreference>> HandleAsync(ResetPreferencesCommand command)
    {
        var currentPreferences = await _preferenceRepository.GetByUserIdAsync(command.UserId);
        var defaultPreferences = CreateDefaultPreferences(command.UserId);
        var resultPreferences = new List<NotificationPreference>();
        
        foreach (var defaultPref in defaultPreferences)
        {
            var existing = currentPreferences.FirstOrDefault(p => p.NotificationType == defaultPref.NotificationType);
            
            if (existing != null)
            {
                existing.IsEnabled = defaultPref.IsEnabled;
                existing.DeliveryMethod = defaultPref.DeliveryMethod;
                existing.UpdatedAt = DateTime.UtcNow;
                await _preferenceRepository.UpdateAsync(existing.Id, existing);
                resultPreferences.Add(existing);
            }
            else
            {
                defaultPref.Id = ObjectId.GenerateNewId().ToString();
                defaultPref.CreatedAt = DateTime.UtcNow;
                defaultPref.UpdatedAt = DateTime.UtcNow;
                await _preferenceRepository.CreateAsync(defaultPref);
                resultPreferences.Add(defaultPref);
            }
        }

        return resultPreferences;
    }

    // 🔥 FIX: Mejorado para manejar múltiples formatos
    private NotificationPreference.ScheduleSettings? ConvertSchedule(object? scheduleObj)
    {
        if (scheduleObj == null) return null;

        try
        {
            // CASO 1: Ya es un JsonElement
            if (scheduleObj is JsonElement jsonElement)
            {
                return ParseJsonElement(jsonElement);
            }

            // CASO 2: Es un objeto serializado como string JSON
            if (scheduleObj is string jsonString)
            {
                var element = JsonSerializer.Deserialize<JsonElement>(jsonString);
                return ParseJsonElement(element);
            }

            // CASO 3: Es un Dictionary u otro objeto
            var json = JsonSerializer.Serialize(scheduleObj);
            var deserializedElement = JsonSerializer.Deserialize<JsonElement>(json);
            return ParseJsonElement(deserializedElement);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error converting schedule: {ex.Message}");
            Console.WriteLine($"Schedule object type: {scheduleObj.GetType().Name}");
            Console.WriteLine($"Schedule object value: {JsonSerializer.Serialize(scheduleObj)}");
            return null;
        }
    }

    // 🆕 Helper: Parsear JsonElement a ScheduleSettings
    private NotificationPreference.ScheduleSettings ParseJsonElement(JsonElement jsonElement)
    {
        var startTime = "09:00";
        var endTime = "22:00";
        var daysOfWeek = new List<int>();

        // Leer start_time
        if (jsonElement.TryGetProperty("start_time", out var startProp))
        {
            startTime = startProp.GetString() ?? "09:00";
        }
        else if (jsonElement.TryGetProperty("StartTime", out var startProp2))
        {
            startTime = startProp2.GetString() ?? "09:00";
        }

        // Leer end_time
        if (jsonElement.TryGetProperty("end_time", out var endProp))
        {
            endTime = endProp.GetString() ?? "22:00";
        }
        else if (jsonElement.TryGetProperty("EndTime", out var endProp2))
        {
            endTime = endProp2.GetString() ?? "22:00";
        }

        // Leer days_of_week
        if (jsonElement.TryGetProperty("days_of_week", out var daysArray))
        {
            foreach (var day in daysArray.EnumerateArray())
            {
                daysOfWeek.Add(day.GetInt32());
            }
        }
        else if (jsonElement.TryGetProperty("DaysOfWeek", out var daysArray2))
        {
            foreach (var day in daysArray2.EnumerateArray())
            {
                daysOfWeek.Add(day.GetInt32());
            }
        }

        // Si no hay días especificados, usar todos
        if (!daysOfWeek.Any())
        {
            daysOfWeek = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
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
                DeliveryMethod = "push",
                Schedule = new NotificationPreference.ScheduleSettings
                {
                    QuietHours = new List<NotificationPreference.ScheduleSettings.QuietHourRange>
                    {
                        new NotificationPreference.ScheduleSettings.QuietHourRange
                        {
                            StartTime = "09:00",
                            EndTime = "09:00"
                        }
                    },
                    ActiveDays = new List<string> { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" },
                    TimeZone = "UTC"
                }
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