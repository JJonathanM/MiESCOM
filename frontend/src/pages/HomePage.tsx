import { useState } from 'react'
import { logout, type User } from '../api/auth'
import { Brand } from '../components/Brand'

type HomePageProps = {
  user: User
  onSignedOut: () => void
}

export function HomePage({ user, onSignedOut }: HomePageProps) {
  const [error, setError] = useState<string>()
  const [isSigningOut, setIsSigningOut] = useState(false)

  async function handleLogout() {
    setError(undefined)
    setIsSigningOut(true)

    try {
      await logout()
      onSignedOut()
    } catch {
      setError('No se pudo cerrar la sesión. Intenta de nuevo.')
      setIsSigningOut(false)
    }
  }

  return (
    <>
      <header className="topbar">
        <Brand />
        <button
          className="button button--inverse"
          type="button"
          onClick={handleLogout}
          disabled={isSigningOut}
        >
          {isSigningOut ? 'Cerrando…' : 'Cerrar sesión'}
        </button>
      </header>

      <main className="home">
        {error && (
          <p className="alert" role="alert">
            {error}
          </p>
        )}
        <h1>Hola, {user.displayName}</h1>
        <p className="muted">Iniciaste sesión con la boleta {user.boleta}.</p>
      </main>
    </>
  )
}
