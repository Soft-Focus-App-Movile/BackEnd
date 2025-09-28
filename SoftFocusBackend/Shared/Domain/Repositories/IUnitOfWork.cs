namespace SoftFocusBackend.Shared.Domain.Repositories;

public interface IUnitOfWork
{
    Task CompleteAsync();
}