# MiESCOM: propuesta de diseño de base de datos

**Estado:** borrador para revisión. Aún no está implementado.
**Motor propuesto:** PostgreSQL.
**Fuente:** exportación de la plataforma institucional (`Ultra.xlsx`, 64 columnas).

## Diagrama

```
 periodo_escolar ─────┐
 unidad_academica ────┤
 programa_academico ──┼──< situacion_academica >──── alumno ──1:1── alumno_datos_personales
 especialidad ────────┘           │                    │
                                  └──< situacion_grupo  └──0..1── usuario ──< usuario_rol >── rol

 importacion ──< importacion_fila       
```

`──<` significa "uno a muchos". `1:1` significa que cada alumno tiene a lo más un registro de datos personales. `0..1` significa que un alumno puede tener o no una cuenta de usuario.

## Tablas

Convenciones: todas las tablas usan un `id` interno como llave primaria. "Origen" indica la columna del Excel de donde sale el dato.

### Catálogos

Se crean a partir de los campos que traen clave y nombre juntos, por ejemplo `63-ESCOM`.

**`periodo_escolar`**

| Columna | Tipo | Nulo | Origen | Notas |
|---|---|---|---|---|
| id | int | no | | |
| clave | varchar(5) | no | Periodo_escolar, Periodo escolar de ingreso | Formato `AAAAN`, por ejemplo `20271`. Única |
| fecha_inicio | date | sí | | Por definir; servirá para las convocatorias de actividades |
| fecha_fin | date | sí | | Por definir |

**`unidad_academica`**

| Columna | Tipo | Nulo | Origen | Notas |
|---|---|---|---|---|
| id | int | no | | |
| clave | varchar(10) | no | Unidad académica | Parte antes del guion: `63`. Única |
| nombre | varchar(150) | no | Unidad académica | Parte después del guion: `ESCOM` |

**`programa_academico`**

| Columna | Tipo | Nulo | Origen | Notas |
|---|---|---|---|---|
| id | int | no | | |
| unidad_academica_id | int | no | | Llave foránea |
| clave | varchar(10) | no | Programa académico | `C` en la muestra. Única por unidad (*por confirmar*) |
| nombre | varchar(200) | no | Programa académico | `ING. EN SIS. COMPUTACIONALES` |

**`especialidad`**

| Columna | Tipo | Nulo | Origen | Notas |
|---|---|---|---|---|
| id | int | no | | |
| programa_academico_id | int | no | | Llave foránea (*por confirmar* si la especialidad depende del programa) |
| clave | varchar(10) | no | Especialidad | `0` en la muestra |
| nombre | varchar(200) | no | Especialidad | `SIN ESPECIALIDAD` |

### `alumno`

Identidad estable del alumno: lo que no cambia de un semestre a otro.

| Columna | Tipo | Nulo | Origen | Notas |
|---|---|---|---|---|
| id | int | no | | |
| boleta | varchar(10) | sí | Boleta | Única. 10 dígitos. Nula mientras el alumno solo tenga pre-boleta (*pendiente*) |
| pre_boleta | varchar(10) | sí | Pre-Boleta | Única. Formato `PE` + 8 dígitos en la muestra |
| apellido_paterno | varchar(100) | no | Apellido Paterno | |
| apellido_materno | varchar(100) | sí | Apellido Materno | Nulo para personas con un solo apellido |
| nombre | varchar(100) | no | Nombre | |
| email_institucional | varchar(254) | sí | Email | En la muestra viene `-`; se guarda como nulo |
| email_personal | varchar(254) | sí | E_MAIL_PERSONAL | |
| periodo_ingreso_id | int | sí | Periodo escolar de ingreso | Llave foránea a `periodo_escolar` |
| creado_en | timestamptz | no | | Auditoría |
| actualizado_en | timestamptz | no | | Auditoría |

Regla: debe existir al menos la boleta o la pre-boleta.

### `alumno_datos_personales`

Relación uno a uno con `alumno`. Separada por ser información sensible.

| Columna | Tipo | Nulo | Origen | Notas |
|---|---|---|---|---|
| alumno_id | int | no | | Llave primaria y foránea |
| curp | char(18) | sí | CURP | Única |
| fecha_nacimiento | date | sí | Fecha_Nac | Viene como texto `AAAA-MM-DD` |
| genero | varchar(20) | sí | Genero | |
| nacionalidad | varchar(50) | sí | Nacionalidad | |
| lugar_nacimiento | varchar(100) | sí | Lugar de Nacimiento | |
| telefono | varchar(20) | sí | Telefono | Vacío en la muestra |
| telefono_celular | varchar(20) | sí | TEL_CELULAR | Vacío en la muestra |
| domicilio | varchar(300) | sí | Domicilio | Vacío en la muestra |
| domicilio_municipio | varchar(100) | sí | Domicilio_Municipio | Vacío en la muestra |
| domicilio_estado | varchar(100) | sí | Domicilio_Estado | Vacío en la muestra |
| nss | varchar(11) | sí | No_Seguro_social | Vacío en la muestra |
| escuela_procedencia | varchar(200) | sí | Escuela Procedencia | Vacío en la muestra |
| entidad_escuela_procedencia | varchar(100) | sí | Entidad Esc Procedencia | Vacío en la muestra |
| promedio_escuela_procedencia | numeric(4,2) | sí | Promedio Esc Procedencia | |

La columna **Edad** no se guarda aparte porque cambia con el tiempo: se calcula a partir de `fecha_nacimiento`. El valor original queda en `importacion_fila`.

### `situacion_academica`

Un registro por alumno y periodo. Combinación única: `alumno_id` + `periodo_escolar_id`.

| Columna | Tipo | Nulo | Origen | Notas |
|---|---|---|---|---|
| id | int | no | | |
| alumno_id | int | no | | Llave foránea |
| periodo_escolar_id | int | no | Periodo_escolar | Llave foránea |
| importacion_id | int | no | | De qué carga salió el registro |
| nivel | varchar(5) | sí | Nivel | `S` en la muestra (*significado por confirmar*) |
| unidad_academica_id | int | sí | Unidad académica | Llave foránea |
| modalidad | varchar(30) | sí | Modalidad | `Escolarizada` |
| turno | varchar(20) | sí | Turno | `MATUTINO` |
| tipo_inscripcion | varchar(20) | sí | Inscrito/Reinscrito | Se normaliza el formato (`ReInscrito`) |
| programa_academico_id | int | sí | Programa académico | Llave foránea |
| plan | varchar(5) | sí | Plan | `20`. Podría volverse catálogo |
| especialidad_id | int | sí | Especialidad | Llave foránea |
| semestre_minimo | smallint | sí | Semestre/Nivel Inscrito Mínimo | |
| semestre_maximo | smallint | sí | Semestre/Nivel Inscrito Máximo | |
| tipo_alumno | char(1) | sí | Tipo_alumno | `I` en la muestra (*por confirmar*: ¿regular/irregular?) |
| creditos_alumno | numeric(6,2) | sí | Créditos del alumno | |
| creditos_especialidad | numeric(6,2) | sí | Créditos de la especialidad | Podría pertenecer al catálogo de especialidad |
| periodos_cursados | smallint | sí | Periodos cursados | |
| promedio | numeric(4,2) | sí | Promedio | |
| materias_inscritas | smallint | sí | Materias inscritas | |
| creditos_inscritos | numeric(6,2) | sí | Créditos inscritos | |
| materias_programa | smallint | sí | Materias del programa académico | Podría pertenecer al catálogo de programa o plan |
| materias_por_acreditar | smallint | sí | # Materias por acreditar | |
| materias_reprobadas | smallint | sí | # Materias Reprobadas | |
| creditos_reprobadas | numeric(6,2) | sí | Créditos Materias Reprobadas | Trae decimales |
| creditos_reprobadas_no_inscritas | numeric(6,2) | sí | Créditos Materias Reprobadas No inscritas | |
| materias_desfasadas | smallint | sí | Materias Desfasadas | |
| periodo_minimo_desfase | varchar(10) | sí | Periodo mínimo de desfase | Vacío en la muestra (*tipo por confirmar*) |
| total_materias_cursadas | smallint | sí | Total de materias cursadas | |
| materias_no_cursadas | smallint | sí | Materias No Cursadas (Saltadas) | |
| total_materias_acreditadas | smallint | sí | Total de materias acreditadas | |
| acreditadas_ordinario | smallint | sí | Materias Acreditadas en ordinario | |
| acreditadas_recursamiento | smallint | sí | Materias Acreditadas en recursamiento | |
| acreditadas_ets | smallint | sí | Materias Acreditadas en ETS | |
| acreditadas_equivalencia | smallint | sí | Materias Acreditadas por equivalencia | |
| acreditadas_movilidad | smallint | sí | Materias Acreditadas por movilidad | |
| porcentaje_avance | numeric(5,2) | sí | % de Avance | |
| carga | varchar(50) | sí | Carga | Texto, por ejemplo `menor a la mínima` |
| carga_inscrita | smallint | sí | Carga Inscrita Alumno | (*significado por confirmar*) |
| creditos_rep_noins | numeric(6,2) | sí | Creditos Rep Noins | Posible duplicado de `creditos_reprobadas_no_inscritas` |
| periodo_maximo | date | sí | Periodo máximo del alumno | Aunque se llama "periodo", trae una fecha (`AAAA-MM-DD`) |


### `situacion_grupo`

Un registro por grupo. Combinación única: `situacion_academica_id` + `grupo` + `tipo`.

| Columna | Tipo | Nulo | Origen | Notas |
|---|---|---|---|---|
| id | int | no | | |
| situacion_academica_id | int | no | | Llave foránea |
| grupo | varchar(10) | no | Grupos Ordinarios, Grupos recursamiento | Por ejemplo `8CM3` |
| tipo | varchar(15) | no | | `ordinario` o `recursamiento`, según la columna de origen |

La columna **Grupos** es la unión de las otras dos (así ocurre en la muestra). El importador lo verifica y registra una advertencia si no coincide; el valor original queda en `importacion_fila`.


### Importaciones

**`importacion`**: una fila por archivo cargado.

| Columna | Tipo | Nulo | Notas |
|---|---|---|---|
| id | int | no | |
| archivo_nombre | varchar(255) | no | |
| archivo_sha256 | char(64) | no | Huella del archivo para detectar cargas duplicadas |
| periodo_escolar_id | int | sí | Periodo al que corresponde la carga |
| importado_por | uuid | no | Llave foránea a `usuario` |
| importado_en | timestamptz | no | |
| total_filas | int | no | |
| filas_con_error | int | no | |

**`importacion_fila`**: una fila por registro del Excel.

| Columna | Tipo | Nulo | Notas |
|---|---|---|---|
| id | int | no | |
| importacion_id | int | no | Llave foránea |
| numero_fila | int | no | Fila en el Excel |
| boleta | varchar(10) | sí | Para buscar rápido |
| datos | jsonb | no | Las 64 columnas tal como vinieron |
| estado | varchar(20) | no | `procesada`, `con_advertencias` o `error` |
| mensaje | text | sí | Detalle del error o advertencia |

Esta tabla duplica datos sensibles, así que conviene definir cuánto tiempo se conserva (por ejemplo, un semestre).


**`usuario`**

| Columna | Tipo | Nulo | Notas |
|---|---|---|---|
| id | uuid | no | |
| nombre_usuario | varchar(50) | no | La boleta, en el caso de los alumnos. Única |
| email | varchar(254) | sí | Correo al que se envían las notificaciones (*pendiente*: institucional o personal) |
| password_hash | text | no | Nunca la contraseña en texto plano (ver el documento de propuestas) |
| password_temporal | boolean | no | Según la propuesta de contraseña autogenerada: obliga a cambiarla al entrar |
| password_expira_en | timestamptz | sí | Caducidad de la contraseña temporal |
| intentos_fallidos | int | no | Para el bloqueo temporal |
| bloqueado_hasta | timestamptz | sí | |
| sello_seguridad | text | no | Cambia al cambiar la contraseña e invalida las sesiones abiertas |
| alumno_id | int | sí | Llave foránea a `alumno`, única. Nula para personal administrativo |
| activo | boolean | no | |
| ultimo_acceso | timestamptz | sí | |
| creado_en | timestamptz | no | |

**`rol`** y **`usuario_rol`**: roles como `alumno` y `administrador` (lista por definir) y su asignación a usuarios.

## Reglas de importación

1. El valor `-` y las celdas vacías se guardan como nulo.
2. Se quitan los espacios sobrantes de todos los textos (`8CM3 ` → `8CM3`).
3. Los campos con formato `clave-nombre` se separan en el primer guion.
4. Las listas de grupos se separan por comas, y cada elemento se guarda como fila.
5. `Fecha_Nac` y `Periodo máximo del alumno` se convierten a fecha.
6. El alumno se busca por boleta: si existe se actualiza; si no, se crea. Queda pendiente cómo tratar a quien solo trae pre-boleta.
7. Si ya existe una situación académica para ese alumno y periodo, se actualiza en lugar de duplicarse.
8. Toda fila, válida o no, se guarda en `importacion_fila` con su estado.


## Anexo: destino de cada columna del Excel

| # | Columna | Destino |
|---|---|---|
| 1 | Periodo_escolar | situacion_academica.periodo_escolar_id |
| 2 | Nivel | situacion_academica.nivel |
| 3 | Unidad académica | unidad_academica + situacion_academica.unidad_academica_id |
| 4 | Modalidad | situacion_academica.modalidad |
| 5 | Apellido Paterno | alumno.apellido_paterno |
| 6 | Apellido Materno | alumno.apellido_materno |
| 7 | Nombre | alumno.nombre |
| 8 | Boleta | alumno.boleta |
| 9 | Pre-Boleta | alumno.pre_boleta |
| 10 | Genero | alumno_datos_personales.genero |
| 11 | Fecha_Nac | alumno_datos_personales.fecha_nacimiento |
| 12 | Edad | Se calcula; el original queda en importacion_fila |
| 13 | CURP | alumno_datos_personales.curp |
| 14 | Nacionalidad | alumno_datos_personales.nacionalidad |
| 15 | Turno | situacion_academica.turno |
| 16 | Telefono | alumno_datos_personales.telefono |
| 17 | TEL_CELULAR | alumno_datos_personales.telefono_celular |
| 18 | Email | alumno.email_institucional |
| 19 | E_MAIL_PERSONAL | alumno.email_personal |
| 20 | Domicilio | alumno_datos_personales.domicilio |
| 21 | Domicilio_Municipio | alumno_datos_personales.domicilio_municipio |
| 22 | Domicilio_Estado | alumno_datos_personales.domicilio_estado |
| 23 | Escuela Procedencia | alumno_datos_personales.escuela_procedencia |
| 24 | Entidad Esc Procedencia | alumno_datos_personales.entidad_escuela_procedencia |
| 25 | Promedio Esc Procedencia | alumno_datos_personales.promedio_escuela_procedencia |
| 26 | Lugar de Nacimiento | alumno_datos_personales.lugar_nacimiento |
| 27 | Periodo escolar de ingreso | alumno.periodo_ingreso_id |
| 28 | Inscrito/Reinscrito | situacion_academica.tipo_inscripcion |
| 29 | Programa académico | programa_academico + situacion_academica.programa_academico_id |
| 30 | Plan | situacion_academica.plan |
| 31 | Especialidad | especialidad + situacion_academica.especialidad_id |
| 32 | Semestre/Nivel Inscrito Mínimo | situacion_academica.semestre_minimo |
| 33 | Semestre/Nivel Inscrito Máximo | situacion_academica.semestre_maximo |
| 34 | Grupos | Se verifica contra 35 y 36; el original queda en importacion_fila |
| 35 | Grupos Ordinarios | situacion_grupo (tipo `ordinario`) |
| 36 | Grupos recursamiento | situacion_grupo (tipo `recursamiento`) |
| 37 | Tipo_alumno | situacion_academica.tipo_alumno |
| 38 | Créditos del alumno | situacion_academica.creditos_alumno |
| 39 | Créditos de la especialidad | situacion_academica.creditos_especialidad |
| 40 | Periodos cursados | situacion_academica.periodos_cursados |
| 41 | Promedio | situacion_academica.promedio |
| 42 | Materias inscritas | situacion_academica.materias_inscritas |
| 43 | Créditos inscritos | situacion_academica.creditos_inscritos |
| 44 | Materias del programa académico | situacion_academica.materias_programa |
| 45 | # Materias por acreditar | situacion_academica.materias_por_acreditar |
| 46 | # Materias Reprobadas | situacion_academica.materias_reprobadas |
| 47 | Créditos Materias Reprobadas | situacion_academica.creditos_reprobadas |
| 48 | Créditos Materias Reprobadas No inscritas | situacion_academica.creditos_reprobadas_no_inscritas |
| 49 | Materias Desfasadas | situacion_academica.materias_desfasadas |
| 50 | Periodo mínimo de desfase | situacion_academica.periodo_minimo_desfase |
| 51 | Total de materias cursadas | situacion_academica.total_materias_cursadas |
| 52 | Materias No Cursadas (Saltadas) | situacion_academica.materias_no_cursadas |
| 53 | Total de materias acreditadas | situacion_academica.total_materias_acreditadas |
| 54 | Materias Acreditadas en ordinario | situacion_academica.acreditadas_ordinario |
| 55 | Materias Acreditadas en recursamiento | situacion_academica.acreditadas_recursamiento |
| 56 | Materias Acreditadas en ETS | situacion_academica.acreditadas_ets |
| 57 | Materias Acreditadas por equivalencia | situacion_academica.acreditadas_equivalencia |
| 58 | Materias Acreditadas por movilidad | situacion_academica.acreditadas_movilidad |
| 59 | % de Avance | situacion_academica.porcentaje_avance |
| 60 | Carga | situacion_academica.carga |
| 61 | Carga Inscrita Alumno | situacion_academica.carga_inscrita |
| 62 | Creditos Rep Noins | situacion_academica.creditos_rep_noins |
| 63 | Periodo máximo del alumno | situacion_academica.periodo_maximo |
| 64 | No_Seguro_social | alumno_datos_personales.nss |
