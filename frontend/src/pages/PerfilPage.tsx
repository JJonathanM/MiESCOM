import { Link } from 'react-router'
import { KeyIcon } from '../components/icons'
import { MiPerfilView } from '../components/MiPerfilView'
import { useSession } from '../hooks/useSession'

export function PerfilPage() {
  const { session } = useSession()
  const idleMinutes = Math.round(session.idleTimeoutMs / 60_000)

  return (
    <div className="page">
      <title>Mi perfil · MiESCOM</title>

      <div className="page__header">
        <h1>Mi perfil</h1>
        <p className="muted">Tus datos según el registro de la escuela.</p>
      </div>

      <MiPerfilView />

      <section className="card" aria-labelledby="seguridad">
        <h2 id="seguridad">Seguridad</h2>
        <p>Tu sesión se cierra automáticamente tras {idleMinutes} minutos sin actividad.</p>
        <Link className="button button--secondary button--with-icon" to="/perfil/contrasena">
          <KeyIcon className="button__icon" />
          Cambiar contraseña
        </Link>
      </section>
    </div>
  )
}
