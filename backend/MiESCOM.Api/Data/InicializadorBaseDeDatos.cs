using Microsoft.EntityFrameworkCore;

namespace MiESCOM.Api.Data;

/// <summary>
/// En desarrollo, al arrancar: aplica las migraciones pendientes y carga los alumnos de prueba.
/// Si la base de datos no responde, la aplicación arranca igual y lo indica en la consola.
/// </summary>
public static class InicializadorBaseDeDatos
{
    public static async Task PrepararParaDesarrolloAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MiEscomDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MiEscomDbContext>>();

        try
        {
            await db.Database.MigrateAsync();
            await DatosDePrueba.CargarAsync(db);
            logger.LogInformation("Base de datos lista (migraciones aplicadas y datos de prueba cargados).");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "No se pudo preparar la base de datos. ¿Está corriendo MySQL (docker compose up -d) y la " +
                "cadena de conexión tiene la contraseña del archivo .env? El inicio de sesión no funcionará hasta resolverlo.");
        }
    }
}
