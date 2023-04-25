using Core.Entities;
using Core.Interfaces;
using Infraestructure.Data;
using Infraestructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(SecurityContext context) : base(context)
    {
    }

    public async Task<User> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.User
                            .Include(u => u.Rols)
                            .Include(u => u.RefreshTokens)
                            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _context.User
                            .Include(u => u.Rols)
                            .Include(u => u.RefreshTokens)
                            .FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());
    }
}

