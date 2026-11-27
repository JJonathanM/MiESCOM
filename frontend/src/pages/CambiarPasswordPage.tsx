import { Link } from 'react-router'
import { PasswordChangeForm } from '../components/PasswordChangeForm'
import { useSession } from '../hooks/useSession'

/** Cambio de contraseña voluntario, desde "Mi perfil". */
export function CambiarPasswordPage() {
  const { session, updateSession } = useSession()

  return (
    <div className="page page--narrow">
      <title>Cambiar contraseña · MiESCOM</title>

      <div className="page__header">
        <Link className="back-link" to="/perfil">
          ← Mi perfil
        </Link>
        <h1>Cambiar contraseña</h1>
        <p className="muted">
          Debe tener al menos 8 caracteres y no contener tu boleta. Al cambiarla se cerrarán tus
          sesiones abiertas en otros dispositivos.
        </p>
      </div>

      <section className="card" aria-label="Cambiar contraseña">
        <PasswordChangeForm boleta={session.user.boleta} isTemporary={false} onChanged={updateSession} />
      </section>
    </div>
  )
}
