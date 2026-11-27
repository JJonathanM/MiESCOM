using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace MiESCOM.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "periodo_escolar",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    clave = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "date", nullable: true),
                    fecha_fin = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_periodo_escolar", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "plan",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    clave = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "programa_academico",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    clave = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_programa_academico", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "alumno",
                columns: table => new
                {
                    boleta = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    pre_boleta = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    primer_apellido = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    segundo_apellido = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    email_institucional = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    email_personal = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    periodo_ingreso_id = table.Column<int>(type: "int", nullable: true),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alumno", x => x.boleta);
                    table.ForeignKey(
                        name: "fk_alumno_periodo_escolar_periodo_ingreso_id",
                        column: x => x.periodo_ingreso_id,
                        principalTable: "periodo_escolar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "importacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    archivo_nombre = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    archivo_sha256 = table.Column<string>(type: "char(64)", fixedLength: true, maxLength: 64, nullable: false),
                    periodo_escolar_id = table.Column<int>(type: "int", nullable: true),
                    importado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    total_filas = table.Column<int>(type: "int", nullable: false),
                    filas_con_error = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_importacion", x => x.id);
                    table.ForeignKey(
                        name: "fk_importacion_periodo_escolar_periodo_escolar_id",
                        column: x => x.periodo_escolar_id,
                        principalTable: "periodo_escolar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "alumno_datos_personales",
                columns: table => new
                {
                    boleta = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    curp = table.Column<string>(type: "char(18)", fixedLength: true, maxLength: 18, nullable: true),
                    nacionalidad = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    telefono_celular = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_alumno_datos_personales", x => x.boleta);
                    table.ForeignKey(
                        name: "fk_alumno_datos_personales_alumno_boleta",
                        column: x => x.boleta,
                        principalTable: "alumno",
                        principalColumn: "boleta",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    boleta = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    password_temporal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    intentos_fallidos = table.Column<int>(type: "int", nullable: false),
                    bloqueado_hasta = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    sello_seguridad = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ultimo_acceso = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.boleta);
                    table.ForeignKey(
                        name: "fk_usuario_alumno_boleta",
                        column: x => x.boleta,
                        principalTable: "alumno",
                        principalColumn: "boleta",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "importacion_fila",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    importacion_id = table.Column<int>(type: "int", nullable: false),
                    numero_fila = table.Column<int>(type: "int", nullable: false),
                    boleta = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    mensaje = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_importacion_fila", x => x.id);
                    table.ForeignKey(
                        name: "fk_importacion_fila_importacion_importacion_id",
                        column: x => x.importacion_id,
                        principalTable: "importacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "situacion_academica",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    boleta = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    periodo_escolar_id = table.Column<int>(type: "int", nullable: false),
                    importacion_id = table.Column<int>(type: "int", nullable: true),
                    turno = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    tipo_inscripcion = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    programa_academico_id = table.Column<int>(type: "int", nullable: true),
                    plan_id = table.Column<int>(type: "int", nullable: true),
                    semestre_minimo = table.Column<short>(type: "smallint", nullable: true),
                    semestre_maximo = table.Column<short>(type: "smallint", nullable: true),
                    tipo_alumno = table.Column<string>(type: "char(1)", fixedLength: true, maxLength: 1, nullable: true),
                    creditos_alumno = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    periodos_cursados = table.Column<short>(type: "smallint", nullable: true),
                    promedio = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    creditos_inscritos = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    materias_reprobadas = table.Column<short>(type: "smallint", nullable: true),
                    materias_desfasadas = table.Column<short>(type: "smallint", nullable: true),
                    total_materias_cursadas = table.Column<short>(type: "smallint", nullable: true),
                    materias_no_cursadas = table.Column<short>(type: "smallint", nullable: true),
                    total_materias_acreditadas = table.Column<short>(type: "smallint", nullable: true),
                    carga = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    carga_inscrita = table.Column<short>(type: "smallint", nullable: true),
                    grupos_ordinarios = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    grupos_recursamiento = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_situacion_academica", x => x.id);
                    table.ForeignKey(
                        name: "fk_situacion_academica_alumno_boleta",
                        column: x => x.boleta,
                        principalTable: "alumno",
                        principalColumn: "boleta",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_situacion_academica_importacion_importacion_id",
                        column: x => x.importacion_id,
                        principalTable: "importacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_situacion_academica_periodo_escolar_periodo_escolar_id",
                        column: x => x.periodo_escolar_id,
                        principalTable: "periodo_escolar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_situacion_academica_plan_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_situacion_academica_programa_academico_programa_academico_id",
                        column: x => x.programa_academico_id,
                        principalTable: "programa_academico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_alumno_periodo_ingreso_id",
                table: "alumno",
                column: "periodo_ingreso_id");

            migrationBuilder.CreateIndex(
                name: "ix_alumno_pre_boleta",
                table: "alumno",
                column: "pre_boleta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_alumno_datos_personales_curp",
                table: "alumno_datos_personales",
                column: "curp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_importacion_archivo_sha256",
                table: "importacion",
                column: "archivo_sha256");

            migrationBuilder.CreateIndex(
                name: "ix_importacion_periodo_escolar_id",
                table: "importacion",
                column: "periodo_escolar_id");

            migrationBuilder.CreateIndex(
                name: "ix_importacion_fila_importacion_id",
                table: "importacion_fila",
                column: "importacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_periodo_escolar_clave",
                table: "periodo_escolar",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plan_clave",
                table: "plan",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_programa_academico_clave",
                table: "programa_academico",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_situacion_academica_boleta_periodo_escolar_id",
                table: "situacion_academica",
                columns: new[] { "boleta", "periodo_escolar_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_situacion_academica_importacion_id",
                table: "situacion_academica",
                column: "importacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_situacion_academica_periodo_escolar_id",
                table: "situacion_academica",
                column: "periodo_escolar_id");

            migrationBuilder.CreateIndex(
                name: "ix_situacion_academica_plan_id",
                table: "situacion_academica",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_situacion_academica_programa_academico_id",
                table: "situacion_academica",
                column: "programa_academico_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alumno_datos_personales");

            migrationBuilder.DropTable(
                name: "importacion_fila");

            migrationBuilder.DropTable(
                name: "situacion_academica");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "importacion");

            migrationBuilder.DropTable(
                name: "plan");

            migrationBuilder.DropTable(
                name: "programa_academico");

            migrationBuilder.DropTable(
                name: "alumno");

            migrationBuilder.DropTable(
                name: "periodo_escolar");
        }
    }
}
