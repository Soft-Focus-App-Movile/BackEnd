using SoftFocusBackend.Auth.Domain.Model.Queries;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Auth.Domain.Services;

public interface IAuthQueryService
{
    Task<AuthenticatedUser?> HandleGetCurrentUserAsync(GetCurrentUserQuery query);
}