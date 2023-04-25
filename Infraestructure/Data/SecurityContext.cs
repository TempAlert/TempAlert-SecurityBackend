using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infraestructure.Data;

public class SecurityContext : DbContext
{
    public SecurityContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> User { get; set; }
    public DbSet<Rol> Rols { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
