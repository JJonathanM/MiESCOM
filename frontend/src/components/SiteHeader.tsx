import type { ReactNode } from 'react'
import escomLogo from '../assets/logos/escom.png'
import ipnLogo from '../assets/logos/ipn-lema.png'

type SiteHeaderProps = {
  /** A la derecha de la franja blanca; por ejemplo, el nombre del alumno. */
  bandEnd?: ReactNode
  /** Al final de la barra azul; por ejemplo, el botón del menú en pantallas chicas. */
  barEnd?: ReactNode
  /** Ocupa todo el ancho (con menú lateral) en lugar de centrarse. */
  wide?: boolean
}

const INSTITUTIONAL_LINKS = [
  { href: 'https://www.ipn.mx', label: 'IPN', name: 'Instituto Politécnico Nacional' },
  { href: 'https://www.escom.ipn.mx', label: 'ESCOM', name: 'Escuela Superior de Cómputo' },
]

/**
 * Encabezado con el estilo de escom.ipn.mx:
 * - Franja guinda arriba, como la de la página, pero sin el logo ni los enlaces del Gobierno
 *   de México: solo enlaces al IPN y a ESCOM.
 * - Franja blanca con el logo del IPN.
 * - Barra azul con el escudo de ESCOM en una pestaña blanca.
 * Al desplazarse, las franjas guinda y blanca se ocultan y la barra azul queda fija arriba.
 * Logos originales en resources/ (fuera del repo).
 */
export function SiteHeader({ bandEnd, barEnd, wide = false }: SiteHeaderProps) {
  const inner = `site-header__inner${wide ? ' site-header__inner--wide' : ''}`

  return (
    <header className="site-header">
      <div className="site-header__strip">
        <div className={inner}>
          <nav className="site-header__links" aria-label="Sitios del IPN">
            {INSTITUTIONAL_LINKS.map(({ href, label, name }) => (
              <a key={href} href={href} target="_blank" rel="noreferrer" aria-label={`${name} (se abre en otra pestaña)`}>
                {label}
              </a>
            ))}
          </nav>
        </div>
      </div>
      <div className="site-header__band">
        <div className={inner}>
          <img
            className="site-header__ipn"
            src={ipnLogo}
            width={370}
            height={112}
            alt="Instituto Politécnico Nacional. La Técnica al Servicio de la Patria"
          />
          {bandEnd}
        </div>
      </div>
      <div className="site-header__bar">
        <div className={inner}>
          <span className="escom-tab">
            <img src={escomLogo} width={149} height={104} alt="Escuela Superior de Cómputo" />
          </span>
          <p className="site-header__title">
            MiESCOM
            <span className="site-header__subtitle">Actividades culturales y deportivas</span>
          </p>
          {barEnd}
        </div>
      </div>
    </header>
  )
}
