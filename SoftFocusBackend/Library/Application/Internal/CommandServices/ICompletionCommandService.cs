using SoftFocusBackend.Library.Domain.Model.Commands;

namespace SoftFocusBackend.Library.Application.Internal.CommandServices;

public interface ICompletionCommandService
{
    Task MarkAsCompletedAsync(MarkAsCompletedCommand command);
}
