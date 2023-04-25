using Core.Entities;
using Core.Interfaces;
using Infraestructure.Data;
using Infraestructure.Repositories;

namespace Infrastructure.Repositories;

public class RolRepository : GenericRepository<Rol>, IRolRepository
{
    public RolRepository(SecurityContext context) : base(context)
    {
    }
}
