import { useEffect, useState, type ReactNode } from 'react'
import { getMiPerfil, type AlumnoPerfil } from '../api/alumnos'
import { errorMessage } from '../api/http'
import { Alert } from './Alert'

const EMPTY = '—'

const dateFormat = new Intl.DateTimeFormat('es-MX', { dateStyle: 'long', timeZone: 'UTC' })

function formatDate(isoDate: string | null) {
  if (!isoDate) {
    return EMPTY
  }
  const [year, month, day] = isoDate.split('-').map(Number)
  return dateFormat.format(new Date(Date.UTC(year, month - 1, day)))
}

function formatSemester(min: number | null, max: number | null) {
  if (min === null && max === null) {
    return EMPTY
  }
  if (min === null || max === null || min === max) {
    return `${min ?? max}°`
  }
  return `${min}° a ${max}°`
}

function Item({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div>
      <dt className="info-label">{label}</dt>
      <dd className="info-value">{children ?? EMPTY}</dd>
    </div>
  )
}

/** Datos del alumno con sesión, leídos de la base de datos. */
export function MiPerfilView() {
  const [perfil, setPerfil] = useState<AlumnoPerfil>()
  const [error, setError] = useState<string>()

  useEffect(() => {
    const controller = new AbortController()

    getMiPerfil(controller.signal)
      .then(setPerfil)
      .catch((cause) => {
        if (!controller.signal.aborted) {
          setError(errorMessage(cause))
        }
      })

    return () => controller.abort()
  }, [])

  if (error) {
    return <Alert notice={{ tone: 'error', message: `No se pudo cargar tu perfil. ${error}` }} />
  }

  if (!perfil) {
    return (
      <p className="muted" role="status">
        Cargando tu información…
      </p>
    )
  }

  const nombreCompleto = [perfil.nombre, perfil.primerApellido, perfil.segundoApellido]
    .filter(Boolean)
    .join(' ')
  const { datosPersonales: personales, situacionAcademica: situacion } = perfil

  return (
    <>
      <section className="card" aria-labelledby="datos-alumno">
        <h2 id="datos-alumno">Datos del alumno</h2>
        <dl className="info-grid">
          <Item label="Nombre completo">{nombreCompleto}</Item>
          <Item label="Boleta">{perfil.boleta}</Item>
          <Item label="Correo institucional">{perfil.emailInstitucional ?? 'No registrado'}</Item>
          <Item label="Correo personal">{perfil.emailPersonal ?? 'No registrado'}</Item>
          <Item label="Periodo de ingreso">{perfil.periodoIngreso}</Item>
        </dl>
      </section>

      <section className="card" aria-labelledby="situacion-academica">
        <h2 id="situacion-academica">
          Situación académica{situacion && ` (${situacion.periodo})`}
        </h2>
        {situacion ? (
          <dl className="info-grid">
            <Item label="Programa académico">{situacion.programaAcademico}</Item>
            <Item label="Plan de estudios">{situacion.plan && `Plan ${situacion.plan}`}</Item>
            <Item label="Turno">{situacion.turno}</Item>
            <Item label="Inscripción">{situacion.tipoInscripcion}</Item>
            <Item label="Semestre">
              {formatSemester(situacion.semestreMinimo, situacion.semestreMaximo)}
            </Item>
            <Item label="Periodos cursados">{situacion.periodosCursados}</Item>
            <Item label="Promedio">{situacion.promedio?.toFixed(2)}</Item>
            <Item label="Créditos acumulados">{situacion.creditosAlumno}</Item>
            <Item label="Créditos inscritos">{situacion.creditosInscritos}</Item>
            <Item label="Grupos">{situacion.gruposOrdinarios.join(', ') || 'Ninguno'}</Item>
            <Item label="Grupos de recursamiento">
              {situacion.gruposRecursamiento.join(', ') || 'Ninguno'}
            </Item>
          </dl>
        ) : (
          <p className="muted">Todavía no hay una situación académica registrada.</p>
        )}
      </section>

      <section className="card" aria-labelledby="datos-personales">
        <h2 id="datos-personales">Datos personales</h2>
        <dl className="info-grid">
          <Item label="CURP">{personales.curp}</Item>
          <Item label="Fecha de nacimiento">{formatDate(personales.fechaNacimiento)}</Item>
          <Item label="Sexo">{personales.sexo}</Item>
          <Item label="Nacionalidad">{personales.nacionalidad}</Item>
          <Item label="Teléfono">{personales.telefono}</Item>
          <Item label="Celular">{personales.telefonoCelular}</Item>
        </dl>
        <p className="card__note">La fecha de nacimiento y el sexo se obtienen de tu CURP.</p>
      </section>
    </>
  )
}
