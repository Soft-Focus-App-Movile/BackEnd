using SoftFocusBackend.Tracking.Application.ACL.Services;
using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Commands;
using SoftFocusBackend.Tracking.Domain.Services;
using SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Tracking.Application.Internal.CommandServices;

public class CheckInCommandService : ICheckInCommandService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly ITrackingDomainService _trackingDomainService;
    private readonly ITrackingNotificationService _trackingNotificationService;
    private readonly ILogger<CheckInCommandService> _logger;

    public CheckInCommandService(
        ICheckInRepository checkInRepository,
        ITrackingDomainService trackingDomainService,
        ITrackingNotificationService trackingNotificationService,
        ILogger<CheckInCommandService> logger)
    {
        _checkInRepository = checkInRepository ?? throw new ArgumentNullException(nameof(checkInRepository));
        _trackingDomainService = trackingDomainService ?? throw new ArgumentNullException(nameof(trackingDomainService));
        _trackingNotificationService = trackingNotificationService ?? throw new ArgumentNullException(nameof(trackingNotificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CheckIn?> HandleCreateCheckInAsync(CreateCheckInCommand command)
    {
        try
        {
            _logger.LogInformation("Processing create check-in command for user: {UserId}", command.UserId);

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid create check-in command for user: {UserId}", command.UserId);
                return null;
            }

            if (await _trackingDomainService.HasUserCompletedCheckInTodayAsync(command.UserId))
            {
                _logger.LogWarning("User already completed check-in today: {UserId}", command.UserId);
                return null;
            }

            var checkIn = await _trackingDomainService.CreateCheckInAsync(
                command.UserId,
                command.EmotionalLevel,
                command.EnergyLevel,
                command.MoodDescription,
                command.SleepHours,
                command.Symptoms,
                command.Notes);

            await _checkInRepository.AddAsync(checkIn);

            // Notify other bounded contexts about the check-in completion
            await _trackingNotificationService.NotifyCheckInCompletedAsync(
                command.UserId, checkIn.Id, command.EmotionalLevel, command.EnergyLevel);
            
            await _trackingNotificationService.NotifyUserEngagementAsync(command.UserId, "CheckInCompleted");

            _logger.LogInformation("Check-in created successfully: {CheckInId} for user: {UserId}", checkIn.Id, command.UserId);
            return checkIn;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating check-in for user: {UserId}", command.UserId);
            return null;
        }
    }

    public async Task<CheckIn?> HandleUpdateCheckInAsync(UpdateCheckInCommand command)
    {
        try
        {
            _logger.LogInformation("Processing update check-in command: {CheckInId}", command.CheckInId);

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid update check-in command: {CheckInId}", command.CheckInId);
                return null;
            }

            var checkIn = await _checkInRepository.FindByIdAsync(command.CheckInId);
            if (checkIn == null)
            {
                _logger.LogWarning("Check-in not found: {CheckInId}", command.CheckInId);
                return null;
            }

            checkIn.UpdateEmotionalState(command.EmotionalLevel, command.EnergyLevel, command.MoodDescription);
            checkIn.UpdateSleepInfo(command.SleepHours);
            checkIn.UpdateSymptoms(command.Symptoms);
            checkIn.UpdateNotes(command.Notes);

            _checkInRepository.Update(checkIn);

            _logger.LogInformation("Check-in updated successfully: {CheckInId}", checkIn.Id);
            return checkIn;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating check-in: {CheckInId}", command.CheckInId);
            return null;
        }
    }

    public async Task<bool> HandleDeleteCheckInAsync(DeleteCheckInCommand command)
    {
        try
        {
            _logger.LogInformation("Processing delete check-in command: {CheckInId}", command.CheckInId);

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid delete check-in command: {CheckInId}", command.CheckInId);
                return false;
            }

            var checkIn = await _checkInRepository.FindByIdAsync(command.CheckInId);
            if (checkIn == null)
            {
                _logger.LogWarning("Check-in not found: {CheckInId}", command.CheckInId);
                return false;
            }

            if (!await _trackingDomainService.CanCheckInBeDeletedAsync(command.CheckInId))
            {
                _logger.LogWarning("Check-in cannot be deleted: {CheckInId}", command.CheckInId);
                return false;
            }

            _checkInRepository.Remove(checkIn);

            _logger.LogInformation("Check-in deleted successfully: {CheckInId}", command.CheckInId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting check-in: {CheckInId}", command.CheckInId);
            return false;
        }
    }
}