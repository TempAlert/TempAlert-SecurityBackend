using Core.Entities;
using Microsoft.Extensions.Logging;

namespace Infraestructure.Data;

public class SecurityContextSeed
{
    public static async Task SeedRolsAsync(SecurityContext context, ILoggerFactory loggerFactory)
    {
        try
        {
            if (!context.Rols.Any())
            {
                var rols = new List<Rol>()
                        {
                            new Rol{Name="Administrator"},
                            new Rol{Name="Employed"},
                        };
                context.Rols.AddRange(rols);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger<SecurityContextSeed>();
            logger.LogError(ex.Message);
        }
    }
}
