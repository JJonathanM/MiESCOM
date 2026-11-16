import { useEffect, useState } from 'react'
import { getCurrentSession, type AuthSession } from './api/auth'
import type { Notice } from './components/Alert'
import { SessionTimeout } from './components/SessionTimeout'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'

type Session =
  | { status: 'checking' }
  | { status: 'signed-out'; notice?: Notice }
  | { status: 'signed-in'; auth: AuthSession }

function App() {
  const [session, setSession] = useState<Session>({ status: 'checking' })

  useEffect(() => {
    const controller = new AbortController()

    getCurrentSession(controller.signal)
      .then((auth) =>
        setSession(auth ? { status: 'signed-in', auth } : { status: 'signed-out' }),
      )
      .catch(() => {
        if (!controller.signal.aborted) {
          setSession({
            status: 'signed-out',
            notice: {
              tone: 'error',
              message: 'No se pudo conectar con el servidor. Intenta de nuevo.',
            },
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
      <>
        <HomePage
          user={session.auth.user}
          onSignedOut={() => setSession({ status: 'signed-out' })}
        />
        <SessionTimeout
          timeoutMs={session.auth.idleTimeoutMs}
          onExpire={() =>
            setSession({
              status: 'signed-out',
              notice: {
                tone: 'info',
                message: 'Tu sesión se cerró por inactividad. Vuelve a iniciar sesión.',
              },
            })
          }
        />
      </>
    )
  }

  return (
    <LoginPage
      notice={session.notice}
      onSignedIn={(auth) => setSession({ status: 'signed-in', auth })}
    />
  )
}

export default App
