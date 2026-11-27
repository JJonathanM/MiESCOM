import { postJson, toApiError } from './http'

export type User = {
  boleta: string
  displayName: string
}

export type AuthSession = {
  user: User
  /** True mientras el alumno no cambie la contraseña temporal que recibió por correo. */
  mustChangePassword: boolean
  /** Tiempo sin actividad tras el cual el servidor cierra la sesión. */
  idleTimeoutMs: number
}

type SessionResponse = User & { mustChangePassword: boolean; idleTimeoutSeconds: number }

export const BOLETA_LENGTH = 10
export const PASSWORD_MIN_LENGTH = 8

export class InvalidCredentialsError extends Error {}
export class InvalidBoletaError extends Error {}

function toAuthSession({ idleTimeoutSeconds, mustChangePassword, ...user }: SessionResponse): AuthSession {
  return { user, mustChangePassword, idleTimeoutMs: idleTimeoutSeconds * 1000 }
}

/**
 * Devuelve la sesión actual, o null si no hay sesión o ya caducó.
 * Cada llamada con sesión válida reinicia el plazo de inactividad en el servidor.
 */
export async function getCurrentSession(signal?: AbortSignal): Promise<AuthSession | null> {
  const response = await fetch('/api/auth/me', { signal })

  if (response.status === 401) {
    return null
  }
  if (!response.ok) {
    throw await toApiError(response)
  }
  return toAuthSession(await response.json())
}

export async function login(boleta: string, password: string): Promise<AuthSession> {
  const response = await postJson('/api/auth/login', { boleta, password })

  if (response.status === 401) {
    throw new InvalidCredentialsError()
  }
  if (response.status === 400) {
    throw new InvalidBoletaError()
  }
  if (!response.ok) {
    // Cuenta bloqueada (423), demasiados intentos (429) o error del servidor.
    throw await toApiError(response)
  }
  return toAuthSession(await response.json())
}

export async function logout(): Promise<void> {
  const response = await fetch('/api/auth/logout', { method: 'POST' })

  if (!response.ok) {
    throw await toApiError(response)
  }
}

/**
 * Crea la cuenta de un alumno inscrito y le envía una contraseña temporal por correo.
 * @returns El correo al que se envió, parcialmente oculto (a*****o@example.com).
 */
export async function registrar(boleta: string): Promise<string> {
  const response = await postJson('/api/auth/registro', { boleta })

  if (response.status === 400) {
    throw new InvalidBoletaError()
  }
  if (!response.ok) {
    throw await toApiError(response)
  }
  const { correo }: { correo: string } = await response.json()
  return correo
}

/** Cambia la contraseña. El servidor cierra las sesiones abiertas en otros dispositivos. */
export async function cambiarPassword(passwordActual: string, passwordNueva: string): Promise<AuthSession> {
  const response = await postJson('/api/auth/cambiar-password', { passwordActual, passwordNueva })

  if (!response.ok) {
    throw await toApiError(response)
  }
  return toAuthSession(await response.json())
}
