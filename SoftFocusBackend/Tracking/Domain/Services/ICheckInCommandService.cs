using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Commands;

namespace SoftFocusBackend.Tracking.Domain.Services;

public interface ICheckInCommandService
{
    Task<CheckIn?> HandleCreateCheckInAsync(CreateCheckInCommand command);
    Task<CheckIn?> HandleUpdateCheckInAsync(UpdateCheckInCommand command);
    Task<bool> HandleDeleteCheckInAsync(DeleteCheckInCommand command);
}