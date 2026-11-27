-- MiESCOM: esquema de la base de datos en MySQL.
--
-- Archivo generado a partir de la migración de Entity Framework Core, solo como referencia
-- para leer el esquema. No lo edites a mano: las tablas se definen en
-- backend/MiESCOM.Api/Data/ y las crea el backend al arrancar en desarrollo.
-- Para regenerarlo después de una migración nueva:
--   dotnet ef migrations script --project backend/MiESCOM.Api -o database/schema.sql
--
-- Los alumnos de prueba (datos inventados) están en backend/MiESCOM.Api/Data/DatosDePrueba.cs.

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `migration_id` varchar(150) NOT NULL,
    `product_version` varchar(32) NOT NULL,
    PRIMARY KEY (`migration_id`)
);

START TRANSACTION;
CREATE TABLE `periodo_escolar` (
    `id` int NOT NULL AUTO_INCREMENT,
    `clave` varchar(5) NOT NULL,
    `fecha_inicio` date NULL,
    `fecha_fin` date NULL,
    PRIMARY KEY (`id`)
);

CREATE TABLE `plan` (
    `id` int NOT NULL AUTO_INCREMENT,
    `clave` varchar(10) NOT NULL,
    `nombre` varchar(100) NULL,
    PRIMARY KEY (`id`)
);

CREATE TABLE `programa_academico` (
    `id` int NOT NULL AUTO_INCREMENT,
    `clave` varchar(10) NOT NULL,
    `nombre` varchar(200) NOT NULL,
    PRIMARY KEY (`id`)
);

CREATE TABLE `alumno` (
    `boleta` varchar(10) NOT NULL,
    `pre_boleta` varchar(10) NULL,
    `primer_apellido` varchar(100) NOT NULL,
    `segundo_apellido` varchar(100) NULL,
    `nombre` varchar(100) NOT NULL,
    `email_institucional` varchar(254) NULL,
    `email_personal` varchar(254) NULL,
    `periodo_ingreso_id` int NULL,
    `creado_en` datetime(6) NOT NULL,
    `actualizado_en` datetime(6) NOT NULL,
    PRIMARY KEY (`boleta`),
    CONSTRAINT `fk_alumno_periodo_escolar_periodo_ingreso_id` FOREIGN KEY (`periodo_ingreso_id`) REFERENCES `periodo_escolar` (`id`) ON DELETE RESTRICT
);

CREATE TABLE `importacion` (
    `id` int NOT NULL AUTO_INCREMENT,
    `archivo_nombre` varchar(255) NOT NULL,
    `archivo_sha256` char(64) NOT NULL,
    `periodo_escolar_id` int NULL,
    `importado_en` datetime(6) NOT NULL,
    `total_filas` int NOT NULL,
    `filas_con_error` int NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_importacion_periodo_escolar_periodo_escolar_id` FOREIGN KEY (`periodo_escolar_id`) REFERENCES `periodo_escolar` (`id`) ON DELETE RESTRICT
);

CREATE TABLE `alumno_datos_personales` (
    `boleta` varchar(10) NOT NULL,
    `curp` char(18) NULL,
    `nacionalidad` varchar(50) NULL,
    `telefono` varchar(20) NULL,
    `telefono_celular` varchar(20) NULL,
    PRIMARY KEY (`boleta`),
    CONSTRAINT `fk_alumno_datos_personales_alumno_boleta` FOREIGN KEY (`boleta`) REFERENCES `alumno` (`boleta`) ON DELETE CASCADE
);

CREATE TABLE `usuario` (
    `boleta` varchar(10) NOT NULL,
    `password_hash` varchar(200) NOT NULL,
    `password_temporal` tinyint(1) NOT NULL,
    `intentos_fallidos` int NOT NULL,
    `bloqueado_hasta` datetime(6) NULL,
    `sello_seguridad` varchar(64) NOT NULL,
    `activo` tinyint(1) NOT NULL,
    `ultimo_acceso` datetime(6) NULL,
    PRIMARY KEY (`boleta`),
    CONSTRAINT `fk_usuario_alumno_boleta` FOREIGN KEY (`boleta`) REFERENCES `alumno` (`boleta`) ON DELETE CASCADE
);

CREATE TABLE `importacion_fila` (
    `id` int NOT NULL AUTO_INCREMENT,
    `importacion_id` int NOT NULL,
    `numero_fila` int NOT NULL,
    `boleta` varchar(10) NULL,
    `mensaje` varchar(500) NOT NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_importacion_fila_importacion_importacion_id` FOREIGN KEY (`importacion_id`) REFERENCES `importacion` (`id`) ON DELETE CASCADE
);

CREATE TABLE `situacion_academica` (
    `id` int NOT NULL AUTO_INCREMENT,
    `boleta` varchar(10) NOT NULL,
    `periodo_escolar_id` int NOT NULL,
    `importacion_id` int NULL,
    `turno` varchar(20) NULL,
    `tipo_inscripcion` varchar(20) NULL,
    `programa_academico_id` int NULL,
    `plan_id` int NULL,
    `semestre_minimo` smallint NULL,
    `semestre_maximo` smallint NULL,
    `tipo_alumno` char(1) NULL,
    `creditos_alumno` decimal(6,2) NULL,
    `periodos_cursados` smallint NULL,
    `promedio` decimal(4,2) NULL,
    `creditos_inscritos` decimal(6,2) NULL,
    `materias_reprobadas` smallint NULL,
    `materias_desfasadas` smallint NULL,
    `total_materias_cursadas` smallint NULL,
    `materias_no_cursadas` smallint NULL,
    `total_materias_acreditadas` smallint NULL,
    `carga` varchar(50) NULL,
    `carga_inscrita` smallint NULL,
    `grupos_ordinarios` varchar(200) NULL,
    `grupos_recursamiento` varchar(200) NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_situacion_academica_alumno_boleta` FOREIGN KEY (`boleta`) REFERENCES `alumno` (`boleta`) ON DELETE CASCADE,
    CONSTRAINT `fk_situacion_academica_importacion_importacion_id` FOREIGN KEY (`importacion_id`) REFERENCES `importacion` (`id`) ON DELETE SET NULL,
    CONSTRAINT `fk_situacion_academica_periodo_escolar_periodo_escolar_id` FOREIGN KEY (`periodo_escolar_id`) REFERENCES `periodo_escolar` (`id`) ON DELETE RESTRICT,
    CONSTRAINT `fk_situacion_academica_plan_plan_id` FOREIGN KEY (`plan_id`) REFERENCES `plan` (`id`) ON DELETE RESTRICT,
    CONSTRAINT `fk_situacion_academica_programa_academico_programa_academico_id` FOREIGN KEY (`programa_academico_id`) REFERENCES `programa_academico` (`id`) ON DELETE RESTRICT
);

CREATE INDEX `ix_alumno_periodo_ingreso_id` ON `alumno` (`periodo_ingreso_id`);

CREATE UNIQUE INDEX `ix_alumno_pre_boleta` ON `alumno` (`pre_boleta`);

CREATE UNIQUE INDEX `ix_alumno_datos_personales_curp` ON `alumno_datos_personales` (`curp`);

CREATE INDEX `ix_importacion_archivo_sha256` ON `importacion` (`archivo_sha256`);

CREATE INDEX `ix_importacion_periodo_escolar_id` ON `importacion` (`periodo_escolar_id`);

CREATE INDEX `ix_importacion_fila_importacion_id` ON `importacion_fila` (`importacion_id`);

CREATE UNIQUE INDEX `ix_periodo_escolar_clave` ON `periodo_escolar` (`clave`);

CREATE UNIQUE INDEX `ix_plan_clave` ON `plan` (`clave`);

CREATE UNIQUE INDEX `ix_programa_academico_clave` ON `programa_academico` (`clave`);

CREATE UNIQUE INDEX `ix_situacion_academica_boleta_periodo_escolar_id` ON `situacion_academica` (`boleta`, `periodo_escolar_id`);

CREATE INDEX `ix_situacion_academica_importacion_id` ON `situacion_academica` (`importacion_id`);

CREATE INDEX `ix_situacion_academica_periodo_escolar_id` ON `situacion_academica` (`periodo_escolar_id`);

CREATE INDEX `ix_situacion_academica_plan_id` ON `situacion_academica` (`plan_id`);

CREATE INDEX `ix_situacion_academica_programa_academico_id` ON `situacion_academica` (`programa_academico_id`);

INSERT INTO `__EFMigrationsHistory` (`migration_id`, `product_version`)
VALUES ('20261127082211_Inicial', '10.0.14');

COMMIT;

