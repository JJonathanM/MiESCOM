using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MiESCOM.Api.Data;

/// <summary>
/// Lo usa "dotnet ef migrations add" para crear migraciones sin conectarse a la base de datos
/// ni necesitar la cadena de conexión real.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MiEscomDbContext>
{
    public MiEscomDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MiEscomDbContext>()
            .UseMySQL("server=localhost;database=miescom;user=miescom;password=solo-para-migraciones")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new MiEscomDbContext(options);
    }
}
