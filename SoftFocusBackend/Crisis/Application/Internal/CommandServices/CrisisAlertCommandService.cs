using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Model.Commands;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Crisis.Domain.Repositories;
using SoftFocusBackend.Crisis.Domain.Services;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Crisis.Application.Internal.CommandServices;

public class CrisisAlertCommandService : ICrisisAlertCommandService
{
    private readonly ICrisisAlertRepository _crisisAlertRepository;
    private readonly ITherapeuticRelationshipRepository _therapeuticRelationshipRepository;
    private readonly ICrisisNotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public CrisisAlertCommandService(
        ICrisisAlertRepository crisisAlertRepository,
        ITherapeuticRelationshipRepository therapeuticRelationshipRepository,
        ICrisisNotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _crisisAlertRepository = crisisAlertRepository;
        _therapeuticRelationshipRepository = therapeuticRelationshipRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrisisAlert> Handle(CreateCrisisAlertCommand command)
    {
        var relationship = (await _therapeuticRelationshipRepository.GetByPatientIdAsync(command.PatientId))
            .FirstOrDefault(r => r.IsActive);

        if (relationship == null)
        {
            throw new InvalidOperationException("Patient does not have an active therapeutic relationship");
        }

        var location = command.Latitude.HasValue && command.Longitude.HasValue
            ? new Location(command.Latitude.Value, command.Longitude.Value)
            : null;

        var emotionalContext = !string.IsNullOrWhiteSpace(command.LastDetectedEmotion)
            ? new EmotionalContext(
                command.LastDetectedEmotion,
                command.LastEmotionDetectedAt,
                command.EmotionSource)
            : EmotionalContext.Empty();

        var alert = new CrisisAlert(
            patientId: command.PatientId,
            psychologistId: relationship.PsychologistId,
            severity: command.Severity,
            triggerSource: command.TriggerSource,
            triggerReason: command.TriggerReason,
            location: location,
            emotionalContext: emotionalContext
        );

        await _crisisAlertRepository.AddAsync(alert);
        await _unitOfWork.CompleteAsync();

        await _notificationService.NotifyPsychologistAsync(alert);

        return alert;
    }

    public async Task<CrisisAlert> Handle(UpdateAlertStatusCommand command)
    {
        var alert = await _crisisAlertRepository.FindByIdAsync(command.AlertId);

        if (alert == null)
        {
            throw new InvalidOperationException($"Crisis alert with id {command.AlertId} not found");
        }

        switch (command.Status)
        {
            case AlertStatus.Attended:
                alert.MarkAsAttended(command.PsychologistNotes);
                break;
            case AlertStatus.Resolved:
                alert.MarkAsResolved(command.PsychologistNotes);
                break;
            case AlertStatus.Dismissed:
                alert.Dismiss(command.PsychologistNotes);
                break;
            default:
                throw new InvalidOperationException($"Invalid status: {command.Status}");
        }

        _crisisAlertRepository.Update(alert);
        await _unitOfWork.CompleteAsync();

        return alert;
    }

    public async Task<CrisisAlert> Handle(UpdateAlertSeverityCommand command)
    {
        var alert = await _crisisAlertRepository.FindByIdAsync(command.AlertId);

        if (alert == null)
        {
            throw new InvalidOperationException($"Crisis alert with id {command.AlertId} not found");
        }

        alert.UpdateSeverity(command.Severity);

        _crisisAlertRepository.Update(alert);
        await _unitOfWork.CompleteAsync();

        return alert;
    }
}
