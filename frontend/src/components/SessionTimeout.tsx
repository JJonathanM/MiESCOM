import { useCallback, useEffect, useRef, useState } from 'react'
import { getCurrentSession } from '../api/auth'

const ACTIVITY_EVENTS = ['pointerdown', 'pointermove', 'keydown', 'wheel', 'scroll', 'touchstart']
const CHECK_INTERVAL_MS = 5_000

type SessionTimeoutProps = {
  /** Debe coincidir con el del servidor; llega en la respuesta de la API. */
  timeoutMs: number
  onExpire: () => void
}

/**
 * Cierra la sesión en pantalla tras un periodo sin actividad, avisando poco antes.
 *
 * Mover el ratón, tocar, teclear o desplazarse cuenta como actividad y renueva la sesión
 * en el servidor (como máximo una vez por minuto). El servidor es quien realmente hace
 * cumplir el límite; aquí solo se mantiene la pantalla sincronizada con él.
 */
export function SessionTimeout({ timeoutMs, onExpire }: SessionTimeoutProps) {
  const [isWarning, setIsWarning] = useState(false)
  const lastRenewalRef = useRef(0)
  const isRenewingRef = useRef(false)
  const onExpireRef = useRef(onExpire)

  // Con 15 minutos: aviso 1 minuto antes y como máximo una renovación por minuto.
  const warningMs = Math.min(60_000, timeoutMs / 4)
  const renewEveryMs = Math.min(60_000, timeoutMs / 4)

  // El componente se monta justo después de iniciar sesión o de consultar la sesión,
  // y ambas cosas acaban de renovar el plazo en el servidor.
  useEffect(() => {
    lastRenewalRef.current = Date.now()
  }, [])

  useEffect(() => {
    onExpireRef.current = onExpire
  }, [onExpire])

  const renew = useCallback(async () => {
    if (isRenewingRef.current) {
      return
    }
    isRenewingRef.current = true
    try {
      if (await getCurrentSession()) {
        lastRenewalRef.current = Date.now()
        setIsWarning(false)
      } else {
        onExpireRef.current()
      }
    } catch {
      // Sin conexión: se conserva el plazo actual y se reintenta con la siguiente actividad.
    } finally {
      isRenewingRef.current = false
    }
  }, [])

  useEffect(() => {
    function handleActivity() {
      if (Date.now() - lastRenewalRef.current >= renewEveryMs) {
        void renew()
      }
    }

    // Se compara contra el reloj en lugar de confiar en un temporizador, porque los
    // navegadores pausan los temporizadores al suspender el equipo o en pestañas ocultas.
    function check() {
      const idleMs = Date.now() - lastRenewalRef.current
      if (idleMs >= timeoutMs) {
        onExpireRef.current()
      } else {
        setIsWarning(idleMs >= timeoutMs - warningMs)
      }
    }

    for (const event of ACTIVITY_EVENTS) {
      window.addEventListener(event, handleActivity, { passive: true })
    }
    document.addEventListener('visibilitychange', check)
    const interval = window.setInterval(check, CHECK_INTERVAL_MS)

    return () => {
      for (const event of ACTIVITY_EVENTS) {
        window.removeEventListener(event, handleActivity)
      }
      document.removeEventListener('visibilitychange', check)
      window.clearInterval(interval)
    }
  }, [timeoutMs, warningMs, renewEveryMs, renew])

  if (!isWarning) {
    return null
  }

  return (
    <div className="session-warning" role="alert">
      <p>Tu sesión se cerrará pronto por inactividad.</p>
      <button className="button button--primary" type="button" onClick={() => void renew()}>
        Seguir conectado
      </button>
    </div>
  )
}
