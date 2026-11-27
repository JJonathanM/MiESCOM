import type { ReactNode } from 'react'
import { SiteFooter } from './SiteFooter'
import { SiteHeader } from './SiteHeader'

type PublicLayoutProps = {
  children: ReactNode
  bandEnd?: ReactNode
}

/** Páginas sin menú lateral: inicio de sesión, registro y cambio obligatorio de contraseña. */
export function PublicLayout({ children, bandEnd }: PublicLayoutProps) {
  return (
    <>
      <a className="skip-link" href="#contenido">
        Saltar al contenido
      </a>
      <SiteHeader bandEnd={bandEnd} />
      <main id="contenido" className="public-main" tabIndex={-1}>
        {children}
      </main>
      <SiteFooter />
    </>
  )
}
