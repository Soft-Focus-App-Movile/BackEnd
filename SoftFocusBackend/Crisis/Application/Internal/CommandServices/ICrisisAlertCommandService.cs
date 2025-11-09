using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Model.Commands;

namespace SoftFocusBackend.Crisis.Application.Internal.CommandServices;

public interface ICrisisAlertCommandService
{
    Task<CrisisAlert> Handle(CreateCrisisAlertCommand command);
    Task<CrisisAlert> Handle(UpdateAlertStatusCommand command);
    Task<CrisisAlert> Handle(UpdateAlertSeverityCommand command);
}
