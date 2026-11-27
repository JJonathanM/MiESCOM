namespace MiESCOM.Api.Data;

/// <summary>Una carga del Excel exportado de la plataforma institucional.</summary>
public sealed class Importacion
{
    public int Id { get; set; }
    public string ArchivoNombre { get; set; } = "";

    /// <summary>Huella del archivo, para detectar cargas duplicadas.</summary>
    public string ArchivoSha256 { get; set; } = "";

    public int? PeriodoEscolarId { get; set; }
    public DateTime ImportadoEn { get; set; }
    public int TotalFilas { get; set; }
    public int FilasConError { get; set; }

    public PeriodoEscolar? PeriodoEscolar { get; set; }
    public List<ImportacionFila> FilasConErrores { get; set; } = [];
}

/// <summary>
/// Registro de las filas que no se pudieron importar. Solo guarda la boleta y el motivo,
/// nunca los datos de la fila.
/// </summary>
public sealed class ImportacionFila
{
    public int Id { get; set; }
    public int ImportacionId { get; set; }
    public int NumeroFila { get; set; }
    public string? Boleta { get; set; }
    public string Mensaje { get; set; } = "";

    public Importacion Importacion { get; set; } = null!;
}
