using SoftFocusBackend.Tracking.Application.ACL.Services;
using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Services;
using SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Tracking.Infrastructure.Services;

public class TrackingDomainService : ITrackingDomainService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly IEmotionalCalendarRepository _emotionalCalendarRepository;
    private readonly IUserValidationService _userValidationService;
    private readonly ILogger<TrackingDomainService> _logger;

    public TrackingDomainService(
        ICheckInRepository checkInRepository,
        IEmotionalCalendarRepository emotionalCalendarRepository,
        IUserValidationService userValidationService,
        ILogger<TrackingDomainService> logger)
    {
        _checkInRepository = checkInRepository ?? throw new ArgumentNullException(nameof(checkInRepository));
        _emotionalCalendarRepository = emotionalCalendarRepository ?? throw new ArgumentNullException(nameof(emotionalCalendarRepository));
        _userValidationService = userValidationService ?? throw new ArgumentNullException(nameof(userValidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> HasUserCompletedCheckInTodayAsync(string userId)
    {
        try
        {
            var todayCheckIn = await _checkInRepository.FindTodayCheckInByUserIdAsync(userId);
            return todayCheckIn != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user completed check-in today: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> HasUserEmotionalCalendarEntryForDateAsync(string userId, DateTime date)
    {
        try
        {
            // Normalize the date to UTC and date only for consistent comparison
            var normalizedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var entry = await _emotionalCalendarRepository.FindByUserIdAndDateAsync(userId, normalizedDate);
            return entry != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user has emotional calendar entry for date: {UserId} - {Date}", userId, date);
            return false;
        }
    }

    public async Task<CheckIn> CreateCheckInAsync(string userId, int emotionalLevel, int energyLevel, 
        string moodDescription, decimal sleepHours, List<string> symptoms, string notes)
    {
        // Validate user exists and is active through ACL
        if (!await _userValidationService.ValidateUserExistsAsync(userId))
        {
            throw new ArgumentException($"User does not exist: {userId}");
        }

        if (!await _userValidationService.IsUserActiveAsync(userId))
        {
            throw new ArgumentException($"User is not active: {userId}");
        }

        var checkIn = new CheckIn
        {
            UserId = userId,
            EmotionalLevel = emotionalLevel,
            EnergyLevel = energyLevel,
            MoodDescription = moodDescription,
            SleepHours = sleepHours,
            Symptoms = symptoms ?? new List<string>(),
            Notes = notes ?? string.Empty,
            CompletedAt = DateTime.UtcNow
        };

        checkIn.ValidateForCreation();
        return checkIn;
    }

    public async Task<EmotionalCalendar> CreateEmotionalCalendarEntryAsync(string userId, DateTime date, 
        string emotionalEmoji, int moodLevel, List<string> emotionalTags)
    {
        // Validate user exists and is active through ACL
        if (!await _userValidationService.ValidateUserExistsAsync(userId))
        {
            throw new ArgumentException($"User does not exist: {userId}");
        }

        if (!await _userValidationService.IsUserActiveAsync(userId))
        {
            throw new ArgumentException($"User is not active: {userId}");
        }

        var entry = new EmotionalCalendar
        {
            UserId = userId,
            Date = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc),
            EmotionalEmoji = emotionalEmoji,
            MoodLevel = moodLevel,
            EmotionalTags = emotionalTags ?? new List<string>()
        };

        entry.ValidateForCreation();
        return entry;
    }

    public async Task<bool> CanCheckInBeDeletedAsync(string checkInId)
    {
        try
        {
            var checkIn = await _checkInRepository.FindByIdAsync(checkInId);
            if (checkIn == null) return false;

            // Business rule: Can only delete check-ins from the last 7 days
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            return checkIn.CreatedAt >= sevenDaysAgo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if check-in can be deleted: {CheckInId}", checkInId);
            return false;
        }
    }

    public async Task<bool> CanEmotionalCalendarEntryBeDeletedAsync(string entryId)
    {
        try
        {
            var entry = await _emotionalCalendarRepository.FindByIdAsync(entryId);
            if (entry == null) return false;

            // Business rule: Can only delete entries from the last 30 days
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            return entry.CreatedAt >= thirtyDaysAgo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if emotional calendar entry can be deleted: {EntryId}", entryId);
            return false;
        }
    }
}