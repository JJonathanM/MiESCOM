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
    nacionalidad: string | null
    telefonoCelular: string | null
    domicilioMunicipio: string | null
    domicilioEstado: string | null
    escuelaProcedencia: string | null
  }
  situacionAcademica: {
    periodoActual: string
    programaAcademico: string
    plan: string
    turno: string
    tipoInscripcion: string
    semestreMinimo: number
    semestreMaximo: number
    promedio: number
    creditosAlumno: number
    creditosEspecialidad: number
    carga: string
    gruposOrdinarios: string[]
    gruposRecursamiento: string[]
  }
}

export async function getMiPerfil(signal?: AbortSignal): Promise<AlumnoPerfil> {
  const response = await fetch('/api/alumnos/me', { signal })

  if (!response.ok) {
    throw new Error(`Error al obtener el perfil: ${response.status}`)
  }

  return response.json()
}