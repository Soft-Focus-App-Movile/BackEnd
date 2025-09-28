using SoftFocusBackend.Shared.Domain.Repositories;

namespace SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public async Task CompleteAsync()
    {
        await Task.CompletedTask;
    }
}