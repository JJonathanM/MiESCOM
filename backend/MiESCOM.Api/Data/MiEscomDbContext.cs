using Microsoft.EntityFrameworkCore;

namespace MiESCOM.Api.Data;

/// <summary>
/// Acceso a MySQL con Entity Framework Core. Los nombres de tablas y columnas se generan
/// en minúsculas con guion bajo (PeriodoEscolar → periodo_escolar), como en el diseño.
/// Para cambiar el esquema: modificar las clases de esta carpeta y crear una migración
/// (ver la guía rápida).
/// </summary>
public sealed class MiEscomDbContext(DbContextOptions<MiEscomDbContext> options) : DbContext(options)
{
    public const string ConnectionStringName = "MiEscom";

    public DbSet<PeriodoEscolar> PeriodosEscolares => Set<PeriodoEscolar>();
    public DbSet<ProgramaAcademico> ProgramasAcademicos => Set<ProgramaAcademico>();
    public DbSet<Plan> Planes => Set<Plan>();
    public DbSet<Alumno> Alumnos => Set<Alumno>();
    public DbSet<AlumnoDatosPersonales> AlumnosDatosPersonales => Set<AlumnoDatosPersonales>();
    public DbSet<SituacionAcademica> SituacionesAcademicas => Set<SituacionAcademica>();
    public DbSet<Importacion> Importaciones => Set<Importacion>();
    public DbSet<ImportacionFila> ImportacionesFilas => Set<ImportacionFila>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Nombres en singular, como en el diseño. Se fijan antes que las relaciones para que
        // los nombres de las llaves foráneas también los usen.
        modelBuilder.Entity<PeriodoEscolar>().ToTable("periodo_escolar");
        modelBuilder.Entity<ProgramaAcademico>().ToTable("programa_academico");
        modelBuilder.Entity<Plan>().ToTable("plan");
        modelBuilder.Entity<Alumno>().ToTable("alumno");
        modelBuilder.Entity<AlumnoDatosPersonales>().ToTable("alumno_datos_personales");
        modelBuilder.Entity<SituacionAcademica>().ToTable("situacion_academica");
        modelBuilder.Entity<Importacion>().ToTable("importacion");
        modelBuilder.Entity<ImportacionFila>().ToTable("importacion_fila");
        modelBuilder.Entity<Usuario>().ToTable("usuario");

        modelBuilder.Entity<PeriodoEscolar>(e =>
        {
            e.Property(p => p.Clave).HasMaxLength(5);
            e.HasIndex(p => p.Clave).IsUnique();
            e.Property(p => p.FechaInicio).HasColumnType("date");
            e.Property(p => p.FechaFin).HasColumnType("date");
        });

        modelBuilder.Entity<ProgramaAcademico>(e =>
        {
            e.Property(p => p.Clave).HasMaxLength(10);
            e.Property(p => p.Nombre).HasMaxLength(200);
            e.HasIndex(p => p.Clave).IsUnique();
        });

        modelBuilder.Entity<Plan>(e =>
        {
            e.Property(p => p.Clave).HasMaxLength(10);
            e.Property(p => p.Nombre).HasMaxLength(100);
            e.HasIndex(p => p.Clave).IsUnique();
        });

        modelBuilder.Entity<Alumno>(e =>
        {
            e.HasKey(a => a.Boleta);
            e.Property(a => a.Boleta).HasMaxLength(10);
            e.Property(a => a.PreBoleta).HasMaxLength(10);
            e.HasIndex(a => a.PreBoleta).IsUnique();
            e.Property(a => a.PrimerApellido).HasMaxLength(100);
            e.Property(a => a.SegundoApellido).HasMaxLength(100);
            e.Property(a => a.Nombre).HasMaxLength(100);
            e.Property(a => a.EmailInstitucional).HasMaxLength(254);
            e.Property(a => a.EmailPersonal).HasMaxLength(254);

            e.HasOne(a => a.PeriodoIngreso)
                .WithMany()
                .HasForeignKey(a => a.PeriodoIngresoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AlumnoDatosPersonales>(e =>
        {
            e.HasKey(d => d.Boleta);
            e.Property(d => d.Boleta).HasMaxLength(10);
            e.Property(d => d.Curp).HasMaxLength(18).IsFixedLength();
            e.HasIndex(d => d.Curp).IsUnique();
            e.Property(d => d.Nacionalidad).HasMaxLength(50);
            e.Property(d => d.Telefono).HasMaxLength(20);
            e.Property(d => d.TelefonoCelular).HasMaxLength(20);

            e.HasOne(d => d.Alumno)
                .WithOne(a => a.DatosPersonales)
                .HasForeignKey<AlumnoDatosPersonales>(d => d.Boleta)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SituacionAcademica>(e =>
        {
            e.Property(s => s.Boleta).HasMaxLength(10);
            e.HasIndex(s => new { s.Boleta, s.PeriodoEscolarId }).IsUnique();
            e.Property(s => s.Turno).HasMaxLength(20);
            e.Property(s => s.TipoInscripcion).HasMaxLength(20);
            e.Property(s => s.TipoAlumno).HasMaxLength(1).IsFixedLength();
            e.Property(s => s.CreditosAlumno).HasPrecision(6, 2);
            e.Property(s => s.Promedio).HasPrecision(4, 2);
            e.Property(s => s.CreditosInscritos).HasPrecision(6, 2);
            e.Property(s => s.Carga).HasMaxLength(50);
            e.Property(s => s.GruposOrdinarios).HasMaxLength(200);
            e.Property(s => s.GruposRecursamiento).HasMaxLength(200);

            e.HasOne(s => s.Alumno)
                .WithMany(a => a.Situaciones)
                .HasForeignKey(s => s.Boleta)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.PeriodoEscolar)
                .WithMany()
                .HasForeignKey(s => s.PeriodoEscolarId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Importacion)
                .WithMany()
                .HasForeignKey(s => s.ImportacionId)
                .HasConstraintName("fk_situacion_academica_importacion_importacion_id")
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(s => s.ProgramaAcademico)
                .WithMany()
                .HasForeignKey(s => s.ProgramaAcademicoId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Plan)
                .WithMany()
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Importacion>(e =>
        {
            e.Property(i => i.ArchivoNombre).HasMaxLength(255);
            e.Property(i => i.ArchivoSha256).HasMaxLength(64).IsFixedLength();
            e.HasIndex(i => i.ArchivoSha256);

            e.HasOne(i => i.PeriodoEscolar)
                .WithMany()
                .HasForeignKey(i => i.PeriodoEscolarId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImportacionFila>(e =>
        {
            e.Property(f => f.Boleta).HasMaxLength(10);
            e.Property(f => f.Mensaje).HasMaxLength(500);

            e.HasOne(f => f.Importacion)
                .WithMany(i => i.FilasConErrores)
                .HasForeignKey(f => f.ImportacionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Boleta);
            e.Property(u => u.Boleta).HasMaxLength(10);
            e.Property(u => u.PasswordHash).HasMaxLength(200);
            e.Property(u => u.SelloSeguridad).HasMaxLength(64);

            e.HasOne(u => u.Alumno)
                .WithOne(a => a.Usuario)
                .HasForeignKey<Usuario>(u => u.Boleta)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
