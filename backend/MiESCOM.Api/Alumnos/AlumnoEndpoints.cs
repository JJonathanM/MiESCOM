using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MiESCOM.Api.Auth;
using MiESCOM.Api.Data;

namespace MiESCOM.Api.Alumnos;

public static class AlumnoEndpoints
{
    public static void MapAlumnoEndpoints(this IEndpointRouteBuilder app)
    {
        // Exige sesión y haber cambiado la contraseña temporal.
        var alumnos = app.MapGroup("/api/alumnos").RequireAuthorization(AuthClaims.PasswordChangedPolicy);

        alumnos.MapGet("/me", GetMiPerfil);
    }

    /// <summary>Datos del alumno con sesión y su situación académica más reciente.</summary>
    private static async Task<IResult> GetMiPerfil(
        ClaimsPrincipal user,
        MiEscomDbContext db,
        CancellationToken cancellationToken)
    {
        var boleta = user.Boleta();

        var alumno = await db.Alumnos
            .AsNoTracking()
            .Include(a => a.PeriodoIngreso)
            .Include(a => a.DatosPersonales)
            .FirstOrDefaultAsync(a => a.Boleta == boleta, cancellationToken);

        if (alumno is null)
        {
            return Results.NotFound();
        }

        var situacion = await db.SituacionesAcademicas
            .AsNoTracking()
            .Include(s => s.PeriodoEscolar)
            .Include(s => s.ProgramaAcademico)
            .Include(s => s.Plan)
            .Where(s => s.Boleta == boleta)
            .OrderByDescending(s => s.PeriodoEscolar.Clave)
            .FirstOrDefaultAsync(cancellationToken);

        var curp = alumno.DatosPersonales?.Curp;

        return Results.Ok(new AlumnoPerfilResponse(
            Boleta: alumno.Boleta,
            Nombre: alumno.Nombre,
            PrimerApellido: alumno.PrimerApellido,
            SegundoApellido: alumno.SegundoApellido,
            EmailInstitucional: alumno.EmailInstitucional,
            EmailPersonal: alumno.EmailPersonal,
            PeriodoIngreso: alumno.PeriodoIngreso?.Clave,
            DatosPersonales: new DatosPersonalesDto(
                Curp: curp,
                FechaNacimiento: Curp.FechaNacimiento(curp),
                Sexo: Curp.Sexo(curp),
                Nacionalidad: alumno.DatosPersonales?.Nacionalidad,
                Telefono: alumno.DatosPersonales?.Telefono,
                TelefonoCelular: alumno.DatosPersonales?.TelefonoCelular),
            SituacionAcademica: situacion is null ? null : new SituacionAcademicaDto(
                Periodo: situacion.PeriodoEscolar.Clave,
                ProgramaAcademico: situacion.ProgramaAcademico?.Nombre,
                Plan: situacion.Plan?.Clave,
                Turno: situacion.Turno,
                TipoInscripcion: situacion.TipoInscripcion,
                SemestreMinimo: situacion.SemestreMinimo,
                SemestreMaximo: situacion.SemestreMaximo,
                PeriodosCursados: situacion.PeriodosCursados,
                Promedio: situacion.Promedio,
                CreditosAlumno: situacion.CreditosAlumno,
                CreditosInscritos: situacion.CreditosInscritos,
                Carga: situacion.Carga,
                GruposOrdinarios: SplitGroups(situacion.GruposOrdinarios),
                GruposRecursamiento: SplitGroups(situacion.GruposRecursamiento))));
    }

    private static string[] SplitGroups(string? grupos) =>
        string.IsNullOrWhiteSpace(grupos)
            ? []
            : grupos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public sealed record AlumnoPerfilResponse(
        string Boleta,
        string Nombre,
        string PrimerApellido,
        string? SegundoApellido,
        string? EmailInstitucional,
        string? EmailPersonal,
        string? PeriodoIngreso,
        DatosPersonalesDto DatosPersonales,
        SituacionAcademicaDto? SituacionAcademica);

    /// <summary>La fecha de nacimiento y el sexo se calculan a partir de la CURP.</summary>
    public sealed record DatosPersonalesDto(
        string? Curp,
        DateOnly? FechaNacimiento,
        string? Sexo,
        string? Nacionalidad,
        string? Telefono,
        string? TelefonoCelular);

    public sealed record SituacionAcademicaDto(
        string Periodo,
        string? ProgramaAcademico,
        string? Plan,
        string? Turno,
        string? TipoInscripcion,
        short? SemestreMinimo,
        short? SemestreMaximo,
        short? PeriodosCursados,
        decimal? Promedio,
        decimal? CreditosAlumno,
        decimal? CreditosInscritos,
        string? Carga,
        string[] GruposOrdinarios,
        string[] GruposRecursamiento);
}
