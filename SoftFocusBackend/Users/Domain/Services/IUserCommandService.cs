using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Commands;

namespace SoftFocusBackend.Users.Domain.Services;

public interface IUserCommandService
{
    Task<User?> HandleCreateUserAsync(CreateUserCommand command);
    Task<User?> HandleUpdateUserProfileAsync(UpdateUserProfileCommand command);
    Task<bool> HandleDeleteUserAsync(DeleteUserCommand command);
}