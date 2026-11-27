import { toApiError } from './http'

export type AlumnoPerfil = {
  boleta: string
  nombre: string
  primerApellido: string
  segundoApellido: string | null
  emailInstitucional: string | null
  emailPersonal: string | null
  periodoIngreso: string | null
  datosPersonales: {
    curp: string | null
    /** Calculada a partir de la CURP (AAAA-MM-DD). */
    fechaNacimiento: string | null
    /** Calculado a partir de la CURP. */
    sexo: string | null
    nacionalidad: string | null
    telefono: string | null
    telefonoCelular: string | null
  }
  /** Situación del periodo más reciente; null si el alumno no tiene ninguna registrada. */
  situacionAcademica: {
    periodo: string
    programaAcademico: string | null
    plan: string | null
    turno: string | null
    tipoInscripcion: string | null
    semestreMinimo: number | null
    semestreMaximo: number | null
    periodosCursados: number | null
    promedio: number | null
    creditosAlumno: number | null
    creditosInscritos: number | null
    carga: string | null
    gruposOrdinarios: string[]
    gruposRecursamiento: string[]
  } | null
}

export async function getMiPerfil(signal?: AbortSignal): Promise<AlumnoPerfil> {
  const response = await fetch('/api/alumnos/me', { signal })

  if (!response.ok) {
    throw await toApiError(response)
  }
  return response.json()
}
