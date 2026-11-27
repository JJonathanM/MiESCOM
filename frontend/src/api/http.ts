const CONNECTION_ERROR = 'No se pudo conectar con el servidor. Intenta de nuevo.'

/** Error de la API con el mensaje que envía el servidor (campo "detail" de ProblemDetails). */
export class ApiError extends Error {
  readonly status: number
  /** Campo del formulario al que se refiere el error, si el servidor lo indica. */
  readonly field?: string

  constructor(status: number, message: string, field?: string) {
    super(message)
    this.status = status
    this.field = field
  }
}

// Respuestas cuyo mensaje escribe la aplicación para el alumno. En los demás errores 5xx
// el mensaje puede traer detalles internos (por ejemplo, de la base de datos), así que
// nunca se muestra.
function isUserFacing(status: number) {
  return status < 500 || status === 503
}

export async function toApiError(response: Response): Promise<ApiError> {
  if (isUserFacing(response.status)) {
    try {
      const body: { detail?: string; campo?: string } = await response.json()
      if (body.detail) {
        return new ApiError(response.status, body.detail, body.campo)
      }
    } catch {
      // La respuesta no es JSON (por ejemplo, el servidor no está corriendo).
    }
  }
  return new ApiError(response.status, CONNECTION_ERROR)
}

/** Mensaje para mostrar en pantalla ante cualquier error de una llamada a la API. */
export function errorMessage(cause: unknown): string {
  return cause instanceof ApiError ? cause.message : CONNECTION_ERROR
}

export async function postJson(url: string, body: unknown): Promise<Response> {
  return fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
}
