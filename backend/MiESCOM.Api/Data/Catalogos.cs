namespace MiESCOM.Api.Data;

/// <summary>Periodo escolar en formato AAAAN, por ejemplo 20271.</summary>
public sealed class PeriodoEscolar
{
    public int Id { get; set; }
    public string Clave { get; set; } = "";

    // Columnas de tipo date. Se usa DateTime porque el conector de MySQL no lee DateOnly.
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
}

/// <summary>Programa académico, por ejemplo "C - ING. EN SIS. COMPUTACIONALES".</summary>
public sealed class ProgramaAcademico
{
    public int Id { get; set; }
    public string Clave { get; set; } = "";
    public string Nombre { get; set; } = "";
}

/// <summary>Plan de estudios, por ejemplo "20".</summary>
public sealed class Plan
{
    public int Id { get; set; }
    public string Clave { get; set; } = "";
    public string? Nombre { get; set; }
}
