export type User = {
  boleta: string
  displayName: string
}

export type AuthSession = {
  user: User
  /** Tiempo sin actividad tras el cual el servidor cierra la sesión. */
  idleTimeoutMs: number
}

type SessionResponse = User & { idleTimeoutSeconds: number }

export const BOLETA_LENGTH = 10

export class InvalidCredentialsError extends Error {}
export class InvalidBoletaError extends Error {}

function toAuthSession({ idleTimeoutSeconds, ...user }: SessionResponse): AuthSession {
  return { user, idleTimeoutMs: idleTimeoutSeconds * 1000 }
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
    throw new Error(`Error del servidor: ${response.status}`)
  }
  return toAuthSession(await response.json())
}

export async function login(boleta: string, password: string): Promise<AuthSession> {
  const response = await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ boleta, password }),
  })

  if (response.status === 401) {
    throw new InvalidCredentialsError()
  }
  if (response.status === 400) {
    throw new InvalidBoletaError()
  }
  if (!response.ok) {
    throw new Error(`Error del servidor: ${response.status}`)
  }
  return toAuthSession(await response.json())
}

export async function logout(): Promise<void> {
  const response = await fetch('/api/auth/logout', { method: 'POST' })

  if (!response.ok) {
    throw new Error(`Error del servidor: ${response.status}`)
  }
}
