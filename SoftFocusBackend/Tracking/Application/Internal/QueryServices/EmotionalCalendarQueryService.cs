using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Queries;
using SoftFocusBackend.Tracking.Domain.Services;
using SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Tracking.Application.Internal.QueryServices;

public class EmotionalCalendarQueryService : IEmotionalCalendarQueryService
{
    private readonly IEmotionalCalendarRepository _emotionalCalendarRepository;
    private readonly ILogger<EmotionalCalendarQueryService> _logger;

    public EmotionalCalendarQueryService(
        IEmotionalCalendarRepository emotionalCalendarRepository,
        ILogger<EmotionalCalendarQueryService> logger)
    {
        _emotionalCalendarRepository = emotionalCalendarRepository ?? throw new ArgumentNullException(nameof(emotionalCalendarRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EmotionalCalendar?> HandleGetEmotionalCalendarEntryByDateAsync(GetEmotionalCalendarEntryByDateQuery query)
    {
        try
        {
            _logger.LogInformation("Processing get emotional calendar entry by date query for user: {UserId}", query.UserId);

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get emotional calendar entry by date query for user: {UserId}", query.UserId);
                return null;
            }

            var entry = await _emotionalCalendarRepository.FindByUserIdAndDateAsync(query.UserId, query.Date);
            
            _logger.LogInformation("Emotional calendar entry by date query completed for user: {UserId} - Found: {Found}", query.UserId, entry != null);
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emotional calendar entry by date for user: {UserId}", query.UserId);
            return null;
        }
    }

    public async Task<List<EmotionalCalendar>> HandleGetUserEmotionalCalendarAsync(GetUserEmotionalCalendarQuery query)
    {
        try
        {
            _logger.LogInformation("Processing get user emotional calendar query for user: {UserId}", query.UserId);

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get user emotional calendar query for user: {UserId}", query.UserId);
                return new List<EmotionalCalendar>();
            }

            var entries = await _emotionalCalendarRepository.FindByUserIdAsync(query.UserId, query.PageNumber, query.PageSize);
            
            _logger.LogInformation("User emotional calendar query completed for user: {UserId} - Count: {Count}", query.UserId, entries.Count);
            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user emotional calendar for user: {UserId}", query.UserId);
            return new List<EmotionalCalendar>();
        }
    }

    public async Task<List<EmotionalCalendar>> HandleGetEmotionalCalendarByDateRangeAsync(GetEmotionalCalendarByDateRangeQuery query)
    {
        try
        {
            _logger.LogInformation("Processing get emotional calendar by date range query for user: {UserId}", query.UserId);

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get emotional calendar by date range query for user: {UserId}", query.UserId);
                return new List<EmotionalCalendar>();
            }

            var entries = await _emotionalCalendarRepository.FindByUserIdAndDateRangeAsync(query.UserId, query.StartDate, query.EndDate);
            
            _logger.LogInformation("Emotional calendar by date range query completed for user: {UserId} - Count: {Count}", query.UserId, entries.Count);
            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emotional calendar by date range for user: {UserId}", query.UserId);
            return new List<EmotionalCalendar>();
        }
    }
}