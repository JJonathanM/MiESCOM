-- Catálogos base
INSERT INTO periodo_escolar (clave, fecha_inicio, fecha_fin)
VALUES ('20271', '2026-08-24', '2027-01-15')
ON CONFLICT (clave) DO NOTHING;

INSERT INTO programa_academico (clave, nombre)
VALUES ('C', 'ING. EN SIS. COMPUTACIONALES')
ON CONFLICT (clave) DO NOTHING;

INSERT INTO plan (clave, nombre)
VALUES ('20', 'Plan de Estudios 2020')
ON CONFLICT (clave) DO NOTHING;

-- Alumno de prueba principal (misma boleta que el login demo)
INSERT INTO alumno (boleta, primer_apellido, segundo_apellido, nombre, email_institucional, email_personal, periodo_ingreso_id)
VALUES (
    '2099630001',
    'Prueba',
    'Demo',
    'Alumno de',
    'aprueba2099@alumno.ipn.mx',
    'alumno.prueba@ejemplo.com',
    (SELECT id FROM periodo_escolar WHERE clave = '20271')
)
ON CONFLICT (boleta) DO NOTHING;

INSERT INTO alumno_datos_personales (boleta, curp, nacionalidad, telefono_celular, domicilio_municipio, domicilio_estado, escuela_procedencia, promedio_escuela_procedencia)
VALUES (
    '2099630001',
    'PRDA030502HDFRRL01',
    'MEXICANA',
    '5500000001',
    'Gustavo A. Madero',
    'Ciudad de México',
    'CECyT 9 Juan de Dios Bátiz',
    8.75
)
ON CONFLICT (boleta) DO NOTHING;

INSERT INTO usuario (boleta, password_hash, password_temporal, sello_seguridad, activo)
VALUES (
    '2099630001',
    'HASH_DEMO_PENDIENTE_IDENTITY',
    FALSE,
    'SELLO-DEMO-0001',
    TRUE
)
ON CONFLICT (boleta) DO NOTHING;

-- Ejemplo de alumno de nuevo ingreso con pre-boleta
INSERT INTO alumno (boleta, primer_apellido, segundo_apellido, nombre, email_personal, periodo_ingreso_id)
VALUES (
    'PE20270001',
    'Nuevo',
    'Ingreso',
    'Aspirante',
    'aspirante.nuevo@ejemplo.com',
    (SELECT id FROM periodo_escolar WHERE clave = '20271')
)
ON CONFLICT (boleta) DO NOTHING;

-- Registro de importación y situación académica de ejemplo
INSERT INTO importacion (archivo_nombre, archivo_sha256, periodo_escolar_id, importado_por, total_filas, filas_con_error)
VALUES (
    'demo_carga_20271.xlsx',
    '0000000000000000000000000000000000000000000000000000000000000001',
    (SELECT id FROM periodo_escolar WHERE clave = '20271'),
    '2099630001',
    2,
    0
);

INSERT INTO situacion_academica (
    boleta, periodo_escolar_id, importacion_id, turno, tipo_inscripcion,
    programa_academico_id, plan_id, semestre_minimo, semestre_maximo, tipo_alumno,
    creditos_alumno, creditos_especialidad, periodos_cursados, promedio,
    materias_inscritas, creditos_inscritos, total_materias_acreditadas, carga
)
VALUES (
    '2099630001',
    (SELECT id FROM periodo_escolar WHERE clave = '20271'),
    (SELECT id FROM importacion ORDER BY id DESC LIMIT 1),
    'MATUTINO',
    'ReInscrito',
    (SELECT id FROM programa_academico WHERE clave = 'C'),
    (SELECT id FROM plan WHERE clave = '20'),
    7, 8, 'R',
    245.50, 320.00, 7, 8.92,
    6, 32.00, 42, 'normal'
)
ON CONFLICT (boleta, periodo_escolar_id) DO NOTHING;

INSERT INTO situacion_grupo (situacion_academica_id, grupo, tipo)
VALUES
    ((SELECT id FROM situacion_academica WHERE boleta = '2099630001' LIMIT 1), '7CM4', 'ordinario'),
    ((SELECT id FROM situacion_academica WHERE boleta = '2099630001' LIMIT 1), '8CM1', 'ordinario')
ON CONFLICT DO NOTHING;