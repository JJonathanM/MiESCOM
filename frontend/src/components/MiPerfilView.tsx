import { useEffect, useState } from 'react'
import { getMiPerfil, type AlumnoPerfil } from '../api/alumnos'

export function MiPerfilView() {
  const [perfil, setPerfil] = useState<AlumnoPerfil | null>(null)
  const [error, setError] = useState<string>()
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const controller = new AbortController()

    getMiPerfil(controller.signal)
      .then((data) => {
        setPerfil(data)
        setLoading(false)
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setError('No se pudo cargar la información de tu perfil.')
          setLoading(false)
        }
      })

    return () => controller.abort()
  }, [])

  if (loading) {
    return <p className="muted">Cargando información del alumno...</p>
  }

  if (error || !perfil) {
    return (
      <p className="alert" role="alert">
        {error ?? 'Información no disponible.'}
      </p>
    )
  }

  const nombreCompleto = [perfil.nombre, perfil.primerApellido, perfil.segundoApellido]
    .filter(Boolean)
    .join(' ')

  return (
    <div className="profile-sections">
      <section className="card">
        <h2>Datos del Alumno</h2>
        <div className="info-grid">
          <div>
            <span className="info-label">Nombre completo</span>
            <p className="info-value">{nombreCompleto}</p>
          </div>
          <div>
            <span className="info-label">Boleta</span>
            <p className="info-value">{perfil.boleta}</p>
          </div>
          <div>
            <span className="info-label">Correo institucional</span>
            <p className="info-value">{perfil.emailInstitucional ?? 'No registrado'}</p>
          </div>
          <div>
            <span className="info-label">Correo personal</span>
            <p className="info-value">{perfil.emailPersonal ?? 'No registrado'}</p>
          </div>
          <div>
            <span className="info-label">Periodo de ingreso</span>
            <p className="info-value">{perfil.periodoIngreso ?? '—'}</p>
          </div>
        </div>
      </section>

      <section className="card">
        <h2>Situación Académica ({perfil.situacionAcademica.periodoActual})</h2>
        <div className="info-grid">
          <div>
            <span className="info-label">Programa académico</span>
            <p className="info-value">{perfil.situacionAcademica.programaAcademico}</p>
          </div>
          <div>
            <span className="info-label">Plan de estudios</span>
            <p className="info-value">Plan {perfil.situacionAcademica.plan}</p>
          </div>
          <div>
            <span className="info-label">Turno / Estado</span>
            <p className="info-value">
              {perfil.situacionAcademica.turno} · {perfil.situacionAcademica.tipoInscripcion}
            </p>
          </div>
          <div>
            <span className="info-label">Semestre</span>
            <p className="info-value">
              {perfil.situacionAcademica.semestreMinimo === perfil.situacionAcademica.semestreMaximo
                ? `${perfil.situacionAcademica.semestreMinimo}°`
                : `${perfil.situacionAcademica.semestreMinimo}° a ${perfil.situacionAcademica.semestreMaximo}°`}
            </p>
          </div>
          <div>
            <span className="info-label">Promedio general</span>
            <p className="info-value">{perfil.situacionAcademica.promedio.toFixed(2)}</p>
          </div>
          <div>
            <span className="info-label">Créditos acumulados</span>
            <p className="info-value">
              {perfil.situacionAcademica.creditosAlumno} / {perfil.situacionAcademica.creditosEspecialidad}
            </p>
          </div>
          <div>
            <span className="info-label">Grupos ordinarios</span>
            <p className="info-value">
              {perfil.situacionAcademica.gruposOrdinarios.join(', ') || 'Ninguno'}
            </p>
          </div>
          <div>
            <span className="info-label">Grupos recursamiento</span>
            <p className="info-value">
              {perfil.situacionAcademica.gruposRecursamiento.join(', ') || 'Ninguno'}
            </p>
          </div>
        </div>
      </section>

      <section className="card">
        <h2>Datos Personales</h2>
        <div className="info-grid">
          <div>
            <span className="info-label">CURP</span>
            <p className="info-value">{perfil.datosPersonales.curp ?? '—'}</p>
          </div>
          <div>
            <span className="info-label">Nacionalidad</span>
            <p className="info-value">{perfil.datosPersonales.nacionalidad ?? '—'}</p>
          </div>
          <div>
            <span className="info-label">Teléfono celular</span>
            <p className="info-value">{perfil.datosPersonales.telefonoCelular ?? '—'}</p>
          </div>
          <div>
            <span className="info-label">Ubicación</span>
            <p className="info-value">
              {[perfil.datosPersonales.domicilioMunicipio, perfil.datosPersonales.domicilioEstado]
                .filter(Boolean)
                .join(', ') || '—'}
            </p>
          </div>
          <div>
            <span className="info-label">Escuela de procedencia</span>
            <p className="info-value">{perfil.datosPersonales.escuelaProcedencia ?? '—'}</p>
          </div>
        </div>
      </section>
    </div>
  )
}