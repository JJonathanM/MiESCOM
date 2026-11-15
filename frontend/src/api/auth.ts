export type User = {
  boleta: string
  displayName: string
}

export const BOLETA_LENGTH = 10

export class InvalidCredentialsError extends Error {}
export class InvalidBoletaError extends Error {}

export async function getCurrentUser(signal?: AbortSignal): Promise<User | null> {
  const response = await fetch('/api/auth/me', { signal })

  if (response.status === 401) {
    return null
  }
  if (!response.ok) {
    throw new Error(`Error del servidor: ${response.status}`)
  }
  return response.json()
}

export async function login(boleta: string, password: string): Promise<User> {
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
  return response.json()
}

export async function logout(): Promise<void> {
  const response = await fetch('/api/auth/logout', { method: 'POST' })

  if (!response.ok) {
    throw new Error(`Error del servidor: ${response.status}`)
  }
}
