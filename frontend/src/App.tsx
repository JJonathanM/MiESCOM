import { useEffect, useState } from 'react'
import { getCurrentUser, type User } from './api/auth'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'

type Session =
  | { status: 'checking' }
  | { status: 'signed-out'; notice?: string }
  | { status: 'signed-in'; user: User }

function App() {
  const [session, setSession] = useState<Session>({ status: 'checking' })

  useEffect(() => {
    const controller = new AbortController()

    getCurrentUser(controller.signal)
      .then((user) =>
        setSession(user ? { status: 'signed-in', user } : { status: 'signed-out' }),
      )
      .catch(() => {
        if (!controller.signal.aborted) {
          setSession({
            status: 'signed-out',
            notice: 'No se pudo conectar con el servidor. Intenta de nuevo.',
          })
        }
      })

    return () => controller.abort()
  }, [])

  if (session.status === 'checking') {
    return null
  }

  if (session.status === 'signed-in') {
    return (
      <HomePage user={session.user} onSignedOut={() => setSession({ status: 'signed-out' })} />
    )
  }

  return (
    <LoginPage
      notice={session.notice}
      onSignedIn={(user) => setSession({ status: 'signed-in', user })}
    />
  )
}

export default App
