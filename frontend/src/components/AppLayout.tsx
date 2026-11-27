import { useEffect, useRef, useState } from 'react'
import { NavLink, Outlet } from 'react-router'
import { logout, type AuthSession } from '../api/auth'
import type { SessionContext } from '../hooks/useSession'
import { Alert } from './Alert'
import { CloseIcon, HomeIcon, LogoutIcon, MenuIcon, UserIcon } from './icons'
import { SiteFooter } from './SiteFooter'
import { SiteHeader } from './SiteHeader'

// Para agregar una sección al menú: añadirla aquí y registrar su ruta en App.tsx.
const NAV_ITEMS = [
  { to: '/inicio', label: 'Inicio', Icon: HomeIcon },
  { to: '/perfil', label: 'Mi perfil', Icon: UserIcon },
]

type AppLayoutProps = {
  session: AuthSession
  onSessionChange: (session: AuthSession) => void
  onSignedOut: () => void
}

// Partículas que no cuentan para las iniciales ("María de la Luz" → "ML").
const NAME_PARTICLES = new Set(['de', 'del', 'la', 'las', 'los', 'y'])

function initials(name: string) {
  return name
    .split(/\s+/)
    .filter((word) => word && !NAME_PARTICLES.has(word.toLowerCase()))
    .slice(0, 2)
    .map((word) => word[0].toUpperCase())
    .join('')
}

/**
 * Estructura genérica de las páginas con sesión: encabezado, menú lateral y contenido.
 * Desde 960 px el menú siempre está visible; en pantallas más chicas se abre con un botón.
 */
export function AppLayout({ session, onSessionChange, onSignedOut }: AppLayoutProps) {
  const [isMenuOpen, setIsMenuOpen] = useState(false)
  const [logoutError, setLogoutError] = useState<string>()
  const [isSigningOut, setIsSigningOut] = useState(false)
  const menuButtonRef = useRef<HTMLButtonElement>(null)
  const sidebarRef = useRef<HTMLElement>(null)
  const { user } = session

  // Al abrir el menú en pantallas chicas: el foco pasa al menú y Escape lo cierra.
  useEffect(() => {
    if (!isMenuOpen) {
      return
    }
    sidebarRef.current?.querySelector<HTMLElement>('a, button')?.focus()

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setIsMenuOpen(false)
        menuButtonRef.current?.focus()
      }
    }
    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [isMenuOpen])

  async function handleLogout() {
    setLogoutError(undefined)
    setIsSigningOut(true)

    try {
      await logout()
      onSignedOut()
    } catch {
      setLogoutError('No se pudo cerrar la sesión. Intenta de nuevo.')
      setIsSigningOut(false)
    }
  }

  const menuButton = (
    <button
      ref={menuButtonRef}
      className="menu-button"
      type="button"
      aria-label={isMenuOpen ? 'Cerrar menú' : 'Abrir menú'}
      aria-expanded={isMenuOpen}
      aria-controls="menu-lateral"
      onClick={() => setIsMenuOpen((open) => !open)}
    >
      {isMenuOpen ? <CloseIcon /> : <MenuIcon />}
    </button>
  )

  const context: SessionContext = { session, updateSession: onSessionChange }

  return (
    <>
      <a className="skip-link" href="#contenido">
        Saltar al contenido
      </a>
      <SiteHeader wide barEnd={menuButton} />

      <div className="app-body">
        <aside
          id="menu-lateral"
          ref={sidebarRef}
          className={`sidebar${isMenuOpen ? ' sidebar--open' : ''}`}
        >
          <div className="sidebar__content">
            <div className="sidebar__user">
              <span className="avatar" aria-hidden="true">
                {initials(user.displayName)}
              </span>
              <div className="sidebar__user-text">
                <p className="sidebar__name">{user.displayName}</p>
                <p className="sidebar__boleta">Boleta {user.boleta}</p>
              </div>
            </div>

            <nav className="sidebar__nav" aria-label="Menú principal">
              <ul>
                {NAV_ITEMS.map(({ to, label, Icon }) => (
                  <li key={to}>
                    <NavLink className="nav-link" to={to} onClick={() => setIsMenuOpen(false)}>
                      <Icon className="nav-link__icon" />
                      {label}
                    </NavLink>
                  </li>
                ))}
              </ul>
            </nav>

            <div className="sidebar__footer">
              {logoutError && <Alert notice={{ tone: 'error', message: logoutError }} />}
              <button
                className="button button--block sidebar__logout"
                type="button"
                onClick={handleLogout}
                disabled={isSigningOut}
              >
                <LogoutIcon className="button__icon" />
                {isSigningOut ? 'Cerrando…' : 'Cerrar sesión'}
              </button>
            </div>
          </div>
        </aside>

        {isMenuOpen && (
          <div className="sidebar-backdrop" aria-hidden="true" onClick={() => setIsMenuOpen(false)} />
        )}

        <main id="contenido" className="main-content" tabIndex={-1}>
          <Outlet context={context} />
        </main>
      </div>
      <SiteFooter />
    </>
  )
}
