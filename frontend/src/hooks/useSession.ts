import { useOutletContext } from 'react-router'
import type { AuthSession } from '../api/auth'

export type SessionContext = {
  session: AuthSession
  /** Reemplaza la sesión en pantalla; por ejemplo, después de cambiar la contraseña. */
  updateSession: (session: AuthSession) => void
}

/** Datos de la sesión para las páginas dentro del menú (los entrega AppLayout). */
export function useSession() {
  return useOutletContext<SessionContext>()
}
