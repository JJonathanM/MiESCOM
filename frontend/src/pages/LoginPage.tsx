import { useState, type FormEvent } from 'react'
import { InvalidCredentialsError, login, type User } from '../api/auth'

type LoginPageProps = {
  notice?: string
  onSignedIn: (user: User) => void
}

export function LoginPage({ notice, onSignedIn }: LoginPageProps) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | undefined>(notice)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(undefined)
    setIsSubmitting(true)

    try {
      onSignedIn(await login(username.trim(), password))
    } catch (cause) {
      setPassword('')
      setError(
        cause instanceof InvalidCredentialsError
          ? 'Usuario o contraseña incorrectos.'
          : 'No se pudo conectar con el servidor. Intenta de nuevo.',
      )
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login">
      <div className="login__brand">
        <p className="brand">MiESCOM</p>
        <p className="muted">Registro de actividades culturales y deportivas</p>
      </div>

      <form className="card" onSubmit={handleSubmit}>
        <h1>Iniciar sesión</h1>

        {error && (
          <p className="alert" role="alert">
            {error}
          </p>
        )}

        <div className="field">
          <label htmlFor="username">Usuario</label>
          <input
            id="username"
            name="username"
            autoComplete="username"
            autoCapitalize="none"
            spellCheck={false}
            required
            autoFocus
            value={username}
            onChange={(event) => setUsername(event.target.value)}
          />
        </div>

        <div className="field">
          <label htmlFor="password">Contraseña</label>
          <input
            id="password"
            name="password"
            type="password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </div>

        <button className="button button--primary" type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Entrando…' : 'Entrar'}
        </button>
      </form>
    </main>
  )
}
