using SoftFocusBackend.Notification.Domain.Model.Queries;
using SoftFocusBackend.Notification.Domain.Model.Aggregates;
using SoftFocusBackend.Notification.Domain.Repositories;

namespace SoftFocusBackend.Notification.Application.Internal.QueryServices;

public class PreferenceQueryService
{
    private readonly INotificationPreferenceRepository _preferenceRepository;

    public PreferenceQueryService(INotificationPreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository;
    }

    public async Task<IEnumerable<NotificationPreference>> HandleAsync(GetPreferencesQuery query)
    {
        return await _preferenceRepository.GetByUserIdAsync(query.UserId);
    }
}