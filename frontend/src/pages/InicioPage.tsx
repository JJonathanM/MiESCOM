import { Link, useLocation } from 'react-router'
import { Alert, type Notice } from '../components/Alert'
import { UserIcon } from '../components/icons'
import { useSession } from '../hooks/useSession'

export function InicioPage() {
  const { session } = useSession()
  // Aviso que deja otra página al redirigir aquí; por ejemplo, tras cambiar la contraseña.
  const notice = (useLocation().state as { notice?: Notice } | null)?.notice

  return (
    <div className="page">
      <title>Inicio · MiESCOM</title>

      <div className="page__header">
        <h1>Bienvenido a MiESCOM, {session.user.displayName}</h1>
        <p className="muted">Registro de actividades culturales y deportivas de ESCOM.</p>
      </div>

      {notice && <Alert notice={notice} />}

      <section className="card" aria-labelledby="proximamente">
        <h2 id="proximamente">Actividades</h2>
        <p>Pronto podrás consultar las convocatorias e inscribirte a actividades desde aquí.</p>
      </section>

      <Link className="shortcut" to="/perfil">
        <UserIcon className="shortcut__icon" />
        <span>
          <span className="shortcut__title">Mi perfil</span>
          <span className="shortcut__text">Consulta tus datos y cambia tu contraseña.</span>
        </span>
      </Link>
    </div>
  )
}
