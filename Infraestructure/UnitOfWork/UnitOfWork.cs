using Core.Interfaces;
using Infraestructure.Data;
using Infrastructure.Repositories;

namespace Infraestructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly SecurityContext _context;
    private IRolRepository _rols;
    private IUserRepository _users;

    public UnitOfWork(SecurityContext context)
    {
        _context = context;
    }

    public IRolRepository Rols
    {
        get
        {
            if (_rols == null)
            {
                _rols = new RolRepository(_context);
            }
            return _rols;
        }
    }

    public IUserRepository Users
    {
        get
        {
            if (_users == null)
            {
                _users = new UserRepository(_context);
            }
            return _users;
        }
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
