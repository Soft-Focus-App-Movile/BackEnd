using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Model.Commands;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Crisis.Domain.Model.Events;
using SoftFocusBackend.Crisis.Domain.Repositories;
using SoftFocusBackend.Crisis.Domain.Services;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Events;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Crisis.Application.Internal.CommandServices;

public class CrisisAlertCommandService : ICrisisAlertCommandService
{
    private readonly ICrisisAlertRepository _crisisAlertRepository;
    private readonly ITherapeuticRelationshipRepository _therapeuticRelationshipRepository;
    private readonly ICrisisNotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventBus _eventBus; // ← NUEVO
    private readonly ILogger<CrisisAlertCommandService> _logger; // ← NUEVO

    public CrisisAlertCommandService(
        ICrisisAlertRepository crisisAlertRepository,
        ITherapeuticRelationshipRepository therapeuticRelationshipRepository,
        ICrisisNotificationService notificationService,
        IUnitOfWork unitOfWork,
        IDomainEventBus eventBus,
        ILogger<CrisisAlertCommandService> logger)
    {
        _crisisAlertRepository = crisisAlertRepository;
        _therapeuticRelationshipRepository = therapeuticRelationshipRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _logger = logger;
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

        // 🔥 NUEVO: Publicar evento de alerta de crisis
        try
        {
            var crisisEvent = new CrisisAlertCreatedEvent(
                alertId: alert.Id,
                patientId: command.PatientId,
                psychologistId: relationship.PsychologistId,
                severity: command.Severity.ToString(),
                triggerSource: command.TriggerSource,
                triggerReason: command.TriggerReason ?? "No especificado"
            );

            await _eventBus.PublishAsync(crisisEvent);

            _logger.LogInformation(
                "CrisisAlertCreatedEvent published for alert {AlertId}",
                alert.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing CrisisAlertCreatedEvent for alert {AlertId}: {Error}",
                alert.Id, ex.Message);
        }

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

// ============================================================================
//                              TESTING NOTES
// ============================================================================
//
// ✔️ TEST 1: Crear alerta de crisis
// - Command enviado: CreateCrisisAlertCommand
// - Se valida si el paciente tiene relación activa → OK
// - Se crea Location solo si viene lat/long → probado con ambas variantes
// - Se crea EmotionalContext vacío o con datos → probado OK
// - Se guarda en el repositorio → alert.Id generado correctamente
// - _unitOfWork.CompleteAsync() confirma la transacción → OK
// - Se ejecuta NotifyPsychologistAsync → mock verificado OK
// - Evento CrisisAlertCreatedEvent publicado → logger confirma publicación
// - Si falla la publicación, no rompe la creación de la alerta → probado OK
//
// Resultado esperado: retorna CrisisAlert con Id y datos completos.
//
// ----------------------------------------------------------------------------
//
// ✔️ TEST 2: Actualizar estado de alerta
// - Command: UpdateAlertStatusCommand
// - Si no existe alerta → lanza InvalidOperationException → OK
// - Cambios posibles:
//      * Attended   → MarkAsAttended()
//      * Resolved   → MarkAsResolved()
//      * Dismissed  → Dismiss()
// - Se persiste con Update() y CompleteAsync() → OK
//
// Resultado esperado: estado actualizado correctamente.
//
// ----------------------------------------------------------------------------
//
// ✔️ TEST 3: Actualizar severidad
// - Command: UpdateAlertSeverityCommand
// - Si el id no existe → excepción correcta
// - Llama alert.UpdateSeverity(newSeverity)
// - Persiste cambios → OK
//
// Resultado esperado: severidad actualizada.
//
// ----------------------------------------------------------------------------
//
// ✔️ TEST 4: Fallos simulados
// - Error al publicar evento → logger captura error, proceso sigue OK
// - Error de repositorio → la creación debe fallar correctamente
//
// ----------------------------------------------------------------------------
//
// ESTADO FINAL: 
// La publicación de eventos es opcional pero no crítica para el flujo principal.
// ============================================================================