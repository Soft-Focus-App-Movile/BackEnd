using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Queries;
using SoftFocusBackend.Tracking.Domain.Services;

namespace SoftFocusBackend.Tracking.Application.Internal.OutboundServices;

public class TrackingFacade : ITrackingFacade
{
    private readonly ICheckInQueryService _checkInQueryService;
    private readonly IEmotionalCalendarQueryService _emotionalCalendarQueryService;
    private readonly ITrackingDomainService _trackingDomainService;
    private readonly ILogger<TrackingFacade> _logger;

    public TrackingFacade(
        ICheckInQueryService checkInQueryService,
        IEmotionalCalendarQueryService emotionalCalendarQueryService,
        ITrackingDomainService trackingDomainService,
        ILogger<TrackingFacade> logger)
    {
        _checkInQueryService = checkInQueryService ?? throw new ArgumentNullException(nameof(checkInQueryService));
        _emotionalCalendarQueryService = emotionalCalendarQueryService ?? throw new ArgumentNullException(nameof(emotionalCalendarQueryService));
        _trackingDomainService = trackingDomainService ?? throw new ArgumentNullException(nameof(trackingDomainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CheckIn?> GetUserTodayCheckInAsync(string userId)
    {
        try
        {
            var query = new GetTodayCheckInQuery { UserId = userId };
            return await _checkInQueryService.HandleGetTodayCheckInAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user today check-in: {UserId}", userId);
            return null;
        }
    }

    public async Task<List<CheckIn>> GetUserCheckInsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = new GetUserCheckInsQuery 
            { 
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = 1,
                PageSize = 100
            };
            return await _checkInQueryService.HandleGetUserCheckInsAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user check-ins: {UserId}", userId);
            return new List<CheckIn>();
        }
    }

    public async Task<EmotionalCalendar?> GetUserEmotionalCalendarEntryByDateAsync(string userId, DateTime date)
    {
        try
        {
            var query = new GetEmotionalCalendarEntryByDateQuery 
            { 
                UserId = userId,
                Date = date.Date
            };
            return await _emotionalCalendarQueryService.HandleGetEmotionalCalendarEntryByDateAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user emotional calendar entry by date: {UserId} - {Date}", userId, date);
            return null;
        }
    }

    public async Task<List<EmotionalCalendar>> GetUserEmotionalCalendarAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                var rangeQuery = new GetEmotionalCalendarByDateRangeQuery
                {
                    UserId = userId,
                    StartDate = startDate.Value,
                    EndDate = endDate.Value
                };
                return await _emotionalCalendarQueryService.HandleGetEmotionalCalendarByDateRangeAsync(rangeQuery);
            }

            var query = new GetUserEmotionalCalendarQuery
            {
                UserId = userId,
                PageNumber = 1,
                PageSize = 100
            };
            return await _emotionalCalendarQueryService.HandleGetUserEmotionalCalendarAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user emotional calendar: {UserId}", userId);
            return new List<EmotionalCalendar>();
        }
    }

    public async Task<bool> HasUserCompletedCheckInTodayAsync(string userId)
    {
        try
        {
            return await _trackingDomainService.HasUserCompletedCheckInTodayAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user completed check-in today: {UserId}", userId);
            return false;
        }
    }
}