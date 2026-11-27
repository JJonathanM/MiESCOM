using Microsoft.EntityFrameworkCore;

namespace MiESCOM.Api.Data;

/// <summary>
/// Alumnos inventados para desarrollo. Solo se cargan en el entorno de desarrollo y si la
/// tabla de alumnos está vacía. Nunca deben agregarse aquí datos reales.
/// Las boletas usan el año 2099 y los correos dominios reservados (example.com, example.edu)
/// para que no coincidan con personas reales.
/// </summary>
public static class DatosDePrueba
{
    public static async Task CargarAsync(MiEscomDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Alumnos.AnyAsync(cancellationToken))
        {
            return;
        }

        var ahora = DateTime.UtcNow;

        var periodo = new PeriodoEscolar
        {
            Clave = "20271",
            FechaInicio = new DateTime(2026, 8, 24),
            FechaFin = new DateTime(2027, 1, 15),
        };
        var periodoIngreso = new PeriodoEscolar { Clave = "20232" };
        var programa = new ProgramaAcademico { Clave = "C", Nombre = "ING. EN SIS. COMPUTACIONALES" };
        var plan = new Plan { Clave = "20", Nombre = "Plan de estudios 2020" };

        db.AddRange(periodo, periodoIngreso, programa, plan);

        // 1. Alumno con correo institucional y situación académica completa.
        db.Alumnos.Add(new Alumno
        {
            Boleta = "2099630001",
            Nombre = "Alumno de",
            PrimerApellido = "Prueba",
            SegundoApellido = "Demo",
            EmailInstitucional = "aprueba@alumno.example.edu",
            EmailPersonal = "alumno.prueba@example.com",
            PeriodoIngreso = periodoIngreso,
            CreadoEn = ahora,
            ActualizadoEn = ahora,
            DatosPersonales = new AlumnoDatosPersonales
            {
                // Nacido el 2 de mayo de 2003 (la "A" del penúltimo carácter indica 2000 o después).
                Curp = "PRDA030502HDFRRLA1",
                Nacionalidad = "MEXICANA",
                TelefonoCelular = "5500000001",
            },
            Situaciones =
            [
                new SituacionAcademica
                {
                    PeriodoEscolar = periodo,
                    Turno = "MATUTINO",
                    TipoInscripcion = "ReInscrito",
                    ProgramaAcademico = programa,
                    Plan = plan,
                    SemestreMinimo = 7,
                    SemestreMaximo = 8,
                    TipoAlumno = "I",
                    CreditosAlumno = 245.50m,
                    PeriodosCursados = 7,
                    Promedio = 8.92m,
                    CreditosInscritos = 40.50m,
                    MateriasReprobadas = 1,
                    MateriasDesfasadas = 1,
                    TotalMateriasCursadas = 45,
                    MateriasNoCursadas = 2,
                    TotalMateriasAcreditadas = 44,
                    Carga = "normal",
                    CargaInscrita = 6,
                    GruposOrdinarios = "7CM4, 8CM1",
                },
            ],
        });

        // 2. Alumna sin correo institucional: el registro usa el correo personal.
        db.Alumnos.Add(new Alumno
        {
            Boleta = "2099630002",
            Nombre = "María de la Luz",
            PrimerApellido = "Ejemplo",
            SegundoApellido = "Ficticia",
            EmailPersonal = "maria.ejemplo@example.com",
            PeriodoIngreso = periodo,
            CreadoEn = ahora,
            ActualizadoEn = ahora,
            DatosPersonales = new AlumnoDatosPersonales
            {
                Curp = "EIFL070815MDFJCZA3",
                Nacionalidad = "MEXICANA",
            },
            Situaciones =
            [
                new SituacionAcademica
                {
                    PeriodoEscolar = periodo,
                    Turno = "VESPERTINO",
                    TipoInscripcion = "Inscrito",
                    ProgramaAcademico = programa,
                    Plan = plan,
                    SemestreMinimo = 1,
                    SemestreMaximo = 1,
                    TipoAlumno = "R",
                    PeriodosCursados = 1,
                    CreditosInscritos = 45.00m,
                    Carga = "normal",
                    CargaInscrita = 6,
                    GruposOrdinarios = "1CV2",
                },
            ],
        });

        // 3. Alumno sin ningún correo: el registro debe rechazarlo con un mensaje claro.
        db.Alumnos.Add(new Alumno
        {
            Boleta = "2099630003",
            Nombre = "Juan",
            PrimerApellido = "Sincorreo",
            PeriodoIngreso = periodo,
            CreadoEn = ahora,
            ActualizadoEn = ahora,
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
