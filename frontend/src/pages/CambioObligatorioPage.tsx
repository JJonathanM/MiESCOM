import { useState } from 'react'
import { logout, type AuthSession } from '../api/auth'
import { Alert } from '../components/Alert'
import { PasswordChangeForm } from '../components/PasswordChangeForm'
import { PublicLayout } from '../components/PublicLayout'

type CambioObligatorioPageProps = {
  session: AuthSession
  onChanged: (session: AuthSession) => void
  onSignedOut: () => void
}

/**
 * Primer inicio de sesión con la contraseña temporal: hay que cambiarla antes de usar
 * cualquier otra sección. El servidor también lo exige.
 */
export function CambioObligatorioPage({ session, onChanged, onSignedOut }: CambioObligatorioPageProps) {
  const [logoutError, setLogoutError] = useState<string>()

  async function handleLogout() {
    setLogoutError(undefined)
    try {
      await logout()
      onSignedOut()
    } catch {
      setLogoutError('No se pudo cerrar la sesión. Intenta de nuevo.')
    }
  }

  const userArea = (
    <div className="band-user">
      <span className="band-user__name">{session.user.displayName}</span>
      <button className="button button--secondary button--small" type="button" onClick={handleLogout}>
        Cerrar sesión
      </button>
    </div>
  )

  return (
    <PublicLayout bandEnd={userArea}>
      <div className="auth-page">
        <title>Cambia tu contraseña · MiESCOM</title>
        <section className="card card--accent form" aria-labelledby="cambio-obligatorio">
          <div className="card__header">
            <h1 id="cambio-obligatorio">Cambia tu contraseña</h1>
            <p className="muted">
              Por seguridad, reemplaza la contraseña temporal que recibiste por correo antes de
              continuar. Debe tener al menos 8 caracteres y no contener tu boleta.
            </p>
          </div>
          {logoutError && <Alert notice={{ tone: 'error', message: logoutError }} />}
          <PasswordChangeForm boleta={session.user.boleta} isTemporary onChanged={onChanged} />
        </section>
      </div>
    </PublicLayout>
  )
}
