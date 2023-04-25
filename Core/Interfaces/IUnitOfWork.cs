namespace Core.Interfaces;

public interface IUnitOfWork
{
    IRolRepository Rols { get; }
    IUserRepository Users { get; }
    public Task<int> SaveAsync();
}
