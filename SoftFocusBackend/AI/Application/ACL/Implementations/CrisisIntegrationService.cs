using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;

namespace SoftFocusBackend.AI.Application.ACL.Implementations;

public class CrisisIntegrationService : ICrisisIntegrationService
{
    private readonly Crisis.Application.ACL.ICrisisIntegrationService _crisisService;
    private readonly ILogger<CrisisIntegrationService> _logger;

    public CrisisIntegrationService(
        Crisis.Application.ACL.ICrisisIntegrationService crisisService,
        ILogger<CrisisIntegrationService> logger)
    {
        _crisisService = crisisService;
        _logger = logger;
    }

    public async Task TriggerCrisisAlertAsync(CrisisAlertRequest request)
    {
        _logger.LogWarning("Crisis alert triggered: UserId={UserId}, Source={Source}, Severity={Severity}, Reason={Reason}",
            request.UserId, request.Source, request.Severity, request.TriggerReason);

        var severity = MapSeverity(request.Severity);

        await _crisisService.CreateAlertFromAIChatAsync(
            request.UserId,
            request.TriggerReason,
            severity
        );
    }

    private AlertSeverity MapSeverity(string severity)
    {
        return severity.ToLower() switch
        {
            "critical" => AlertSeverity.Critical,
            "high" => AlertSeverity.High,
            "moderate" => AlertSeverity.Moderate,
            _ => AlertSeverity.Moderate
        };
    }
}
