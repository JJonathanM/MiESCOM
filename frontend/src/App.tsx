import { useEffect, useState } from 'react'
import { Navigate, Route, Routes, useNavigate } from 'react-router'
import { getCurrentSession, type AuthSession } from './api/auth'
import type { Notice } from './components/Alert'
import { AppLayout } from './components/AppLayout'
import { PublicLayout } from './components/PublicLayout'
import { SessionTimeout } from './components/SessionTimeout'
import { CambiarPasswordPage } from './pages/CambiarPasswordPage'
import { CambioObligatorioPage } from './pages/CambioObligatorioPage'
import { InicioPage } from './pages/InicioPage'
import { LoginPage } from './pages/LoginPage'
import { PerfilPage } from './pages/PerfilPage'
import { RegistroPage } from './pages/RegistroPage'

type Session =
  | { status: 'checking' }
  | { status: 'signed-out'; notice?: Notice }
  | { status: 'signed-in'; auth: AuthSession }

const INACTIVITY_NOTICE: Notice = {
  tone: 'info',
  message: 'Tu sesión se cerró por inactividad. Vuelve a iniciar sesión.',
}

function App() {
  const [session, setSession] = useState<Session>({ status: 'checking' })
  const navigate = useNavigate()

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
            notice: { tone: 'error', message: 'No se pudo conectar con el servidor. Intenta de nuevo.' },
          })
        }
      })

    return () => controller.abort()
  }, [])

  if (session.status === 'checking') {
    return null
  }

  const signIn = (auth: AuthSession) => setSession({ status: 'signed-in', auth })
  const signOut = (notice?: Notice) => setSession({ status: 'signed-out', notice })

  // Sin sesión: registro en /registro y login en cualquier otra dirección. Al entrar se
  // conserva la dirección que se pidió (por ejemplo, /perfil).
  if (session.status === 'signed-out') {
    return (
      <Routes>
        <Route
          path="/registro"
          element={
            <PublicLayout>
              <RegistroPage />
            </PublicLayout>
          }
        />
        <Route
          path="*"
          element={
            <PublicLayout>
              <LoginPage notice={session.notice} onSignedIn={signIn} />
            </PublicLayout>
          }
        />
      </Routes>
    )
  }

  const { auth } = session
  const sessionTimeout = (
    <SessionTimeout timeoutMs={auth.idleTimeoutMs} onExpire={() => signOut(INACTIVITY_NOTICE)} />
  )

  // Con la contraseña temporal solo se puede cambiarla; el servidor también lo exige.
  if (auth.mustChangePassword) {
    return (
      <>
        <CambioObligatorioPage
          session={auth}
          onChanged={(updated) => {
            signIn(updated)
            navigate('/inicio', {
              replace: true,
              state: { notice: { tone: 'success', message: 'Listo. Tu contraseña se actualizó.' } },
            })
          }}
          onSignedOut={() => signOut()}
        />
        {sessionTimeout}
      </>
    )
  }

  return (
    <>
      <Routes>
        <Route
          element={<AppLayout session={auth} onSessionChange={signIn} onSignedOut={() => signOut()} />}
        >
          <Route path="/inicio" element={<InicioPage />} />
          <Route path="/perfil" element={<PerfilPage />} />
          <Route path="/perfil/contrasena" element={<CambiarPasswordPage />} />
          <Route path="*" element={<Navigate to="/inicio" replace />} />
        </Route>
      </Routes>
      {sessionTimeout}
    </>
  )
}

export default App
