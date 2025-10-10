using SoftFocusBackend.Library.Domain.Model.Commands;

namespace SoftFocusBackend.Library.Application.Internal.CommandServices;

public interface IAssignmentCommandService
{
    Task<List<string>> AssignContentAsync(AssignContentCommand command);
}
