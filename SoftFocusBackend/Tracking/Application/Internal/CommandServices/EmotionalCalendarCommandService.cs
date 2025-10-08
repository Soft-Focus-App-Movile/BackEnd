using SoftFocusBackend.Tracking.Application.ACL.Services;
using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Commands;
using SoftFocusBackend.Tracking.Domain.Services;
using SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Tracking.Application.Internal.CommandServices;

public class EmotionalCalendarCommandService : IEmotionalCalendarCommandService
{
    private readonly IEmotionalCalendarRepository _emotionalCalendarRepository;
    private readonly ITrackingDomainService _trackingDomainService;
    private readonly ITrackingNotificationService _trackingNotificationService;
    private readonly ILogger<EmotionalCalendarCommandService> _logger;

    public EmotionalCalendarCommandService(
        IEmotionalCalendarRepository emotionalCalendarRepository,
        ITrackingDomainService trackingDomainService,
        ITrackingNotificationService trackingNotificationService,
        ILogger<EmotionalCalendarCommandService> logger)
    {
        _emotionalCalendarRepository = emotionalCalendarRepository ?? throw new ArgumentNullException(nameof(emotionalCalendarRepository));
        _trackingDomainService = trackingDomainService ?? throw new ArgumentNullException(nameof(trackingDomainService));
        _trackingNotificationService = trackingNotificationService ?? throw new ArgumentNullException(nameof(trackingNotificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EmotionalCalendar?> HandleCreateEmotionalCalendarEntryAsync(CreateEmotionalCalendarEntryCommand command)
    {
        try
        {
            _logger.LogInformation("Processing create emotional calendar entry command for user: {UserId}", command.UserId);

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid create emotional calendar entry command for user: {UserId}", command.UserId);
                return null;
            }

            if (await _trackingDomainService.HasUserEmotionalCalendarEntryForDateAsync(command.UserId, command.Date))
            {
                _logger.LogWarning("User already has emotional calendar entry for date: {UserId} - {Date}", command.UserId, command.Date);
                return null;
            }

            var entry = await _trackingDomainService.CreateEmotionalCalendarEntryAsync(
                command.UserId,
                command.Date,
                command.EmotionalEmoji,
                command.MoodLevel,
                command.EmotionalTags);

            await _emotionalCalendarRepository.AddAsync(entry);

            // Notify other bounded contexts about the emotional calendar entry
            await _trackingNotificationService.NotifyEmotionalCalendarEntryCreatedAsync(
                command.UserId, entry.Id, command.Date, command.MoodLevel);
            
            await _trackingNotificationService.NotifyUserEngagementAsync(command.UserId, "EmotionalCalendarEntry");

            _logger.LogInformation("Emotional calendar entry created successfully: {EntryId} for user: {UserId}", entry.Id, command.UserId);
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating emotional calendar entry for user: {UserId}", command.UserId);
            return null;
        }
    }

    public async Task<EmotionalCalendar?> HandleUpdateEmotionalCalendarEntryAsync(UpdateEmotionalCalendarEntryCommand command)
    {
        try
        {
            _logger.LogInformation("Processing update emotional calendar entry command: {EntryId}", command.EntryId);

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid update emotional calendar entry command: {EntryId}", command.EntryId);
                return null;
            }

            var entry = await _emotionalCalendarRepository.FindByIdAsync(command.EntryId);
            if (entry == null)
            {
                _logger.LogWarning("Emotional calendar entry not found: {EntryId}", command.EntryId);
                return null;
            }

            entry.UpdateEmotionalEntry(command.EmotionalEmoji, command.MoodLevel, command.EmotionalTags);

            _emotionalCalendarRepository.Update(entry);

            _logger.LogInformation("Emotional calendar entry updated successfully: {EntryId}", entry.Id);
            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating emotional calendar entry: {EntryId}", command.EntryId);
            return null;
        }
    }

    public async Task<bool> HandleDeleteEmotionalCalendarEntryAsync(DeleteEmotionalCalendarEntryCommand command)
    {
        try
        {
            _logger.LogInformation("Processing delete emotional calendar entry command: {EntryId}", command.EntryId);

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid delete emotional calendar entry command: {EntryId}", command.EntryId);
                return false;
            }

            var entry = await _emotionalCalendarRepository.FindByIdAsync(command.EntryId);
            if (entry == null)
            {
                _logger.LogWarning("Emotional calendar entry not found: {EntryId}", command.EntryId);
                return false;
            }

            if (!await _trackingDomainService.CanEmotionalCalendarEntryBeDeletedAsync(command.EntryId))
            {
                _logger.LogWarning("Emotional calendar entry cannot be deleted: {EntryId}", command.EntryId);
                return false;
            }

            _emotionalCalendarRepository.Remove(entry);

            _logger.LogInformation("Emotional calendar entry deleted successfully: {EntryId}", command.EntryId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting emotional calendar entry: {EntryId}", command.EntryId);
            return false;
        }
    }
}