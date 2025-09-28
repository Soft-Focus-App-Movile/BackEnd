using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Queries;

namespace SoftFocusBackend.Users.Domain.Services;

public interface IUserQueryService
{
    Task<User?> HandleGetUserByIdAsync(GetUserByIdQuery query);
    Task<User?> HandleGetUserByEmailAsync(GetUserByEmailQuery query);
    Task<(List<User> Users, int TotalCount)> HandleGetAllUsersAsync(GetAllUsersQuery query);
}
