-- 1. Catálogos
CREATE TABLE periodo_escolar (
    id SERIAL PRIMARY KEY,
    clave VARCHAR(5) NOT NULL UNIQUE,
    fecha_inicio DATE NULL,
    fecha_fin DATE NULL
);

CREATE TABLE programa_academico (
    id SERIAL PRIMARY KEY,
    clave VARCHAR(10) NOT NULL UNIQUE,
    nombre VARCHAR(200) NOT NULL
);

CREATE TABLE plan (
    id SERIAL PRIMARY KEY,
    clave VARCHAR(10) NOT NULL UNIQUE,
    nombre VARCHAR(100) NULL
);

-- 2. Alumno y tablas 1:1
CREATE TABLE alumno (
    boleta VARCHAR(10) PRIMARY KEY,
    primer_apellido VARCHAR(100) NOT NULL,
    segundo_apellido VARCHAR(100) NULL,
    nombre VARCHAR(100) NOT NULL,
    email_institucional VARCHAR(254) NULL,
    email_personal VARCHAR(254) NULL,
    periodo_ingreso_id INT NULL REFERENCES periodo_escolar(id),
    creado_en TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    actualizado_en TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE alumno_datos_personales (
    boleta VARCHAR(10) PRIMARY KEY REFERENCES alumno(boleta) ON UPDATE CASCADE ON DELETE CASCADE,
    curp CHAR(18) NULL UNIQUE,
    nacionalidad VARCHAR(50) NULL,
    telefono VARCHAR(20) NULL,
    telefono_celular VARCHAR(20) NULL,
    domicilio VARCHAR(300) NULL,
    domicilio_municipio VARCHAR(100) NULL,
    domicilio_estado VARCHAR(100) NULL,
    nss VARCHAR(11) NULL,
    escuela_procedencia VARCHAR(200) NULL,
    entidad_escuela_procedencia VARCHAR(100) NULL,
    promedio_escuela_procedencia NUMERIC(4,2) NULL
);

CREATE TABLE usuario (
    boleta VARCHAR(10) PRIMARY KEY REFERENCES alumno(boleta) ON UPDATE CASCADE ON DELETE CASCADE,
    password_hash TEXT NOT NULL,
    password_temporal BOOLEAN NOT NULL DEFAULT TRUE,
    password_expira_en TIMESTAMPTZ NULL,
    sello_seguridad TEXT NOT NULL,
    activo BOOLEAN NOT NULL DEFAULT TRUE,
    ultimo_acceso TIMESTAMPTZ NULL,
    creado_en TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 3. Importaciones
CREATE TABLE importacion (
    id SERIAL PRIMARY KEY,
    archivo_nombre VARCHAR(255) NOT NULL,
    archivo_sha256 CHAR(64) NOT NULL,
    periodo_escolar_id INT NULL REFERENCES periodo_escolar(id),
    importado_por VARCHAR(10) NULL,
    importado_en TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    total_filas INT NOT NULL,
    filas_con_error INT NOT NULL
);

CREATE TABLE importacion_error (
    id SERIAL PRIMARY KEY,
    importacion_id INT NOT NULL REFERENCES importacion(id) ON DELETE CASCADE,
    numero_fila INT NOT NULL,
    boleta VARCHAR(10) NULL,
    columna_excel VARCHAR(100) NULL,
    valor_recibido TEXT NULL,
    mensaje TEXT NOT NULL
);

-- 4. Situación académica y grupos
CREATE TABLE situacion_academica (
    id SERIAL PRIMARY KEY,
    boleta VARCHAR(10) NOT NULL REFERENCES alumno(boleta) ON UPDATE CASCADE ON DELETE CASCADE,
    periodo_escolar_id INT NOT NULL REFERENCES periodo_escolar(id),
    importacion_id INT NOT NULL REFERENCES importacion(id),
    turno VARCHAR(20) NULL,
    tipo_inscripcion VARCHAR(20) NULL,
    programa_academico_id INT NULL REFERENCES programa_academico(id),
    plan_id INT NULL REFERENCES plan(id),
    semestre_minimo SMALLINT NULL,
    semestre_maximo SMALLINT NULL,
    tipo_alumno CHAR(1) NULL,
    creditos_alumno NUMERIC(6,2) NULL,
    creditos_especialidad NUMERIC(6,2) NULL,
    periodos_cursados SMALLINT NULL,
    promedio NUMERIC(4,2) NULL,
    materias_inscritas SMALLINT NULL,
    creditos_inscritos NUMERIC(6,2) NULL,
    materias_programa SMALLINT NULL,
    materias_por_acreditar SMALLINT NULL,
    materias_reprobadas SMALLINT NULL,
    creditos_reprobadas NUMERIC(6,2) NULL,
    creditos_reprobadas_no_inscritas NUMERIC(6,2) NULL,
    materias_desfasadas SMALLINT NULL,
    periodo_minimo_desfase VARCHAR(10) NULL,
    total_materias_cursadas SMALLINT NULL,
    materias_no_cursadas SMALLINT NULL,
    total_materias_acreditadas SMALLINT NULL,
    acreditadas_ordinario SMALLINT NULL,
    acreditadas_recursamiento SMALLINT NULL,
    acreditadas_ets SMALLINT NULL,
    acreditadas_equivalencia SMALLINT NULL,
    acreditadas_movilidad SMALLINT NULL,
    carga VARCHAR(50) NULL,
    carga_inscrita SMALLINT NULL,
    creditos_rep_noins NUMERIC(6,2) NULL,
    periodo_maximo DATE NULL,
    CONSTRAINT uq_situacion_alumno_periodo UNIQUE (boleta, periodo_escolar_id)
);

CREATE TABLE situacion_grupo (
    id SERIAL PRIMARY KEY,
    situacion_academica_id INT NOT NULL REFERENCES situacion_academica(id) ON DELETE CASCADE,
    grupo VARCHAR(10) NOT NULL,
    tipo VARCHAR(15) NOT NULL,
    CONSTRAINT uq_situacion_grupo UNIQUE (situacion_academica_id, grupo, tipo)
);