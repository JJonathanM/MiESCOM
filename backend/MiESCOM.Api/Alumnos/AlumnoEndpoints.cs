using System.Security.Claims;

namespace MiESCOM.Api.Alumnos;

public static class AlumnoEndpoints
{
    public static void MapAlumnoEndpoints(this IEndpointRouteBuilder app)
    {
        var alumnos = app.MapGroup("/api/alumnos").RequireAuthorization();

        alumnos.MapGet("/me", GetMiPerfil);
    }

    private static IResult GetMiPerfil(ClaimsPrincipal user)
    {
        var boleta = user.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(boleta))
        {
            return Results.Unauthorized();
        }

        // Datos de ejemplo alineados con el nuevo esquema de PostgreSQL
        // (cuando conectemos EF Core, esto se leerá directo de alumno, alumno_datos_personales y situacion_academica)
        var perfil = new AlumnoPerfilResponse(
            Boleta: boleta,
            Nombre: "Alumno de",
            PrimerApellido: "Prueba",
            SegundoApellido: "Demo",
            EmailInstitucional: "aprueba2099@alumno.ipn.mx",
            EmailPersonal: "alumno.prueba@ejemplo.com",
            PeriodoIngreso: "20271",
            DatosPersonales: new DatosPersonalesDto(
                Curp: "PRDA030502HDFRRL01",
                Nacionalidad: "MEXICANA",
                TelefonoCelular: "5500000001",
                DomicilioMunicipio: "Gustavo A. Madero",
                DomicilioEstado: "Ciudad de México",
                EscuelaProcedencia: "CECyT 9 Juan de Dios Bátiz"
            ),
            SituacionAcademica: new SituacionAcademicaDto(
                PeriodoActual: "20271",
                ProgramaAcademico: "ING. EN SIS. COMPUTACIONALES",
                Plan: "20",
                Turno: "MATUTINO",
                TipoInscripcion: "ReInscrito",
                SemestreMinimo: 7,
                SemestreMaximo: 8,
                Promedio: 8.92m,
                CreditosAlumno: 245.50m,
                CreditosEspecialidad: 320.00m,
                Carga: "normal",
                GruposOrdinarios: ["7CM4", "8CM1"],
                GruposRecursamiento: []
            )
        );

        return Results.Ok(perfil);
    }

    public sealed record AlumnoPerfilResponse(
        string Boleta,
        string Nombre,
        string PrimerApellido,
        string? SegundoApellido,
        string? EmailInstitucional,
        string? EmailPersonal,
        string? PeriodoIngreso,
        DatosPersonalesDto DatosPersonales,
        SituacionAcademicaDto SituacionAcademica
    );

    public sealed record DatosPersonalesDto(
        string? Curp,
        string? Nacionalidad,
        string? TelefonoCelular,
        string? DomicilioMunicipio,
        string? DomicilioEstado,
        string? EscuelaProcedencia
    );

    public sealed record SituacionAcademicaDto(
        string PeriodoActual,
        string ProgramaAcademico,
        string Plan,
        string Turno,
        string TipoInscripcion,
        short SemestreMinimo,
        short SemestreMaximo,
        decimal Promedio,
        decimal CreditosAlumno,
        decimal CreditosEspecialidad,
        string Carga,
        string[] GruposOrdinarios,
        string[] GruposRecursamiento
    );
}