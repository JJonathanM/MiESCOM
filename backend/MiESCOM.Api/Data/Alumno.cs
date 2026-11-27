namespace MiESCOM.Api.Data;

/// <summary>
/// Identidad estable del alumno. La llave primaria es la boleta.
/// El sexo y la fecha de nacimiento no se guardan: se obtienen de la CURP.
/// </summary>
public sealed class Alumno
{
    public string Boleta { get; set; } = "";
    public string? PreBoleta { get; set; }
    public string PrimerApellido { get; set; } = "";
    public string? SegundoApellido { get; set; }
    public string Nombre { get; set; } = "";
    public string? EmailInstitucional { get; set; }
    public string? EmailPersonal { get; set; }
    public int? PeriodoIngresoId { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }

    public PeriodoEscolar? PeriodoIngreso { get; set; }
    public AlumnoDatosPersonales? DatosPersonales { get; set; }
    public List<SituacionAcademica> Situaciones { get; set; } = [];
    public Usuario? Usuario { get; set; }
}

/// <summary>Datos personales del alumno, separados por ser información sensible (1:1).</summary>
public sealed class AlumnoDatosPersonales
{
    public string Boleta { get; set; } = "";
    public string? Curp { get; set; }
    public string? Nacionalidad { get; set; }
    public string? Telefono { get; set; }
    public string? TelefonoCelular { get; set; }

    public Alumno Alumno { get; set; } = null!;
}

/// <summary>Situación académica del alumno en un periodo. Única por boleta y periodo.</summary>
public sealed class SituacionAcademica
{
    public int Id { get; set; }
    public string Boleta { get; set; } = "";
    public int PeriodoEscolarId { get; set; }
    public int? ImportacionId { get; set; }
    public string? Turno { get; set; }
    public string? TipoInscripcion { get; set; }
    public int? ProgramaAcademicoId { get; set; }
    public int? PlanId { get; set; }
    public short? SemestreMinimo { get; set; }
    public short? SemestreMaximo { get; set; }
    public string? TipoAlumno { get; set; }
    public decimal? CreditosAlumno { get; set; }
    public short? PeriodosCursados { get; set; }
    public decimal? Promedio { get; set; }
    public decimal? CreditosInscritos { get; set; }
    public short? MateriasReprobadas { get; set; }
    public short? MateriasDesfasadas { get; set; }
    public short? TotalMateriasCursadas { get; set; }
    public short? MateriasNoCursadas { get; set; }
    public short? TotalMateriasAcreditadas { get; set; }
    public string? Carga { get; set; }
    public short? CargaInscrita { get; set; }

    /// <summary>Lista separada por comas, tal como viene en el Excel ("7CM4, 8CM1").</summary>
    public string? GruposOrdinarios { get; set; }

    /// <summary>Lista separada por comas, tal como viene en el Excel.</summary>
    public string? GruposRecursamiento { get; set; }

    public Alumno Alumno { get; set; } = null!;
    public PeriodoEscolar PeriodoEscolar { get; set; } = null!;
    public Importacion? Importacion { get; set; }
    public ProgramaAcademico? ProgramaAcademico { get; set; }
    public Plan? Plan { get; set; }
}
