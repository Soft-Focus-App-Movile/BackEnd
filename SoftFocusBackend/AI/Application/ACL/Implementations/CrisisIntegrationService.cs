using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;

namespace SoftFocusBackend.AI.Application.ACL.Implementations;

public class CrisisIntegrationService : ICrisisIntegrationService
{
    private readonly ILogger<CrisisIntegrationService> _logger;

    public CrisisIntegrationService(ILogger<CrisisIntegrationService> logger)
    {
        _logger = logger;
    }

    public Task TriggerCrisisAlertAsync(CrisisAlertRequest request)
    {
        _logger.LogWarning("Crisis context not implemented yet. Crisis alert logged: UserId={UserId}, Source={Source}, Severity={Severity}, Reason={Reason}",
            request.UserId, request.Source, request.Severity, request.TriggerReason);

        _logger.LogWarning("CRISIS ALERT (MOCK): User {UserId} - Severity: {Severity} - {Context}",
            request.UserId, request.Severity, request.Context);

        return Task.CompletedTask;
    }
}
