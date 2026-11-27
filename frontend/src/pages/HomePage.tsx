import { useState } from 'react'
import { logout, type User } from '../api/auth'
import { Brand } from '../components/Brand'
import { MiPerfilView } from '../components/MiPerfilView'

type HomePageProps = {
  user: User
  onSignedOut: () => void
}

type MenuSection = 'inicio' | 'perfil'

export function HomePage({ user, onSignedOut }: HomePageProps) {
  const [activeSection, setActiveSection] = useState<MenuSection>('inicio')
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [error, setError] = useState<string>()
  const [isSigningOut, setIsSigningOut] = useState(false)

  function handleSelectSection(section: MenuSection) {
    setActiveSection(section)
    setIsSidebarOpen(false)
  }

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
    <div className="app-shell">
      <header className="topbar">
        <div className="topbar__left">
          <button
            type="button"
            className="menu-toggle"
            aria-label="Abrir menú lateral"
            aria-expanded={isSidebarOpen}
            onClick={() => setIsSidebarOpen((prev) => !prev)}
          >
            <span className="menu-toggle__icon" aria-hidden="true">
              ☰
            </span>
          </button>

          <div className="institutional-logos">
            <img
              src="/logos/ipn.png"
              alt="Escudo del Instituto Politécnico Nacional"
              className="institutional-logo"
            />
            <span className="institutional-divider" aria-hidden="true" />
            <img
              src="/logos/escom.png"
              alt="Escudo de la Escuela Superior de Cómputo"
              className="institutional-logo"
            />
          </div>

          <Brand />
        </div>

        <button
          className="button button--inverse"
          type="button"
          onClick={handleLogout}
          disabled={isSigningOut}
        >
          {isSigningOut ? 'Cerrando…' : 'Cerrar sesión'}
        </button>
      </header>

      {/* Fondo oscuro al abrir el menú */}
      {isSidebarOpen && (
        <div
          className="sidebar-backdrop"
          aria-hidden="true"
          onClick={() => setIsSidebarOpen(false)}
        />
      )}

      {/* Barra lateral deslizable */}
      <aside
        className={`sidebar-drawer ${isSidebarOpen ? 'sidebar-drawer--open' : ''}`}
        aria-label="Menú principal"
      >
        <div className="sidebar-drawer__top">
          <div className="sidebar-drawer__logos">
            <img src="/logos/ipn.png" alt="IPN" className="sidebar-drawer__logo" />
            <img src="/logos/escom.png" alt="ESCOM" className="sidebar-drawer__logo" />
          </div>
          <button
            type="button"
            className="sidebar-drawer__close"
            aria-label="Cerrar menú lateral"
            onClick={() => setIsSidebarOpen(false)}
          >
            ✕
          </button>
        </div>

        <div className="sidebar__header">
          <span className="sidebar__title">Alumno activo</span>
          <span className="sidebar__boleta">{user.boleta}</span>
        </div>

        <nav className="sidebar__nav">
          <button
            type="button"
            className={`sidebar__link ${activeSection === 'inicio' ? 'sidebar__link--active' : ''}`}
            onClick={() => handleSelectSection('inicio')}
          >
            Inicio
          </button>

          <button
            type="button"
            className={`sidebar__link ${activeSection === 'perfil' ? 'sidebar__link--active' : ''}`}
            onClick={() => handleSelectSection('perfil')}
          >
            Mi perfil
          </button>
        </nav>
      </aside>

      <main className="dashboard-content">
        {error && (
          <p className="alert" role="alert">
            {error}
          </p>
        )}

        {activeSection === 'inicio' ? (
          <section className="card">
            <h1>Bienvenido a MiESCOM, {user.displayName}</h1>
            <p className="muted">
              Iniciaste sesión con la boleta <strong>{user.boleta}</strong>. Presiona el botón{' '}
              <strong>☰</strong> en la esquina superior izquierda para desplegar el menú y
              consultar <strong>Mi perfil</strong>.
            </p>
          </section>
        ) : (
          <>
            <h1 className="page-title">Mi perfil</h1>
            <MiPerfilView />
          </>
        )}
      </main>
    </div>
  )
}