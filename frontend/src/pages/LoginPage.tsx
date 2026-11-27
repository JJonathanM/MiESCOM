import { useRef, useState, type FormEvent } from 'react'
import { Link } from 'react-router'
import {
  BOLETA_LENGTH,
  InvalidBoletaError,
  InvalidCredentialsError,
  login,
  type AuthSession,
} from '../api/auth'
import { errorMessage } from '../api/http'
import { Alert, type Notice } from '../components/Alert'

const BOLETA_ERROR = `La boleta debe tener ${BOLETA_LENGTH} dígitos.`
// Ejemplo del formato; el año 2099 no coincide con ninguna boleta real.
const BOLETA_EXAMPLE = '2099630001'

type LoginPageProps = {
  notice?: Notice
  onSignedIn: (auth: AuthSession) => void
}

export function LoginPage({ notice, onSignedIn }: LoginPageProps) {
  const [boleta, setBoleta] = useState('')
  const [password, setPassword] = useState('')
  const [message, setMessage] = useState<Notice | undefined>(notice)
  const [isBoletaInvalid, setIsBoletaInvalid] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const boletaRef = useRef<HTMLInputElement>(null)
  const passwordRef = useRef<HTMLInputElement>(null)

  function showError(text: string) {
    setMessage({ tone: 'error', message: text })
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (boleta.length !== BOLETA_LENGTH) {
      setIsBoletaInvalid(true)
      showError(BOLETA_ERROR)
      boletaRef.current?.focus()
      return
    }
    if (password === '') {
      showError('Escribe tu contraseña.')
      passwordRef.current?.focus()
      return
    }

    setIsBoletaInvalid(false)
    setMessage(undefined)
    setIsSubmitting(true)

    try {
      onSignedIn(await login(boleta, password))
    } catch (cause) {
      setPassword('')
      if (cause instanceof InvalidCredentialsError) {
        showError('Boleta o contraseña incorrectas.')
      } else if (cause instanceof InvalidBoletaError) {
        setIsBoletaInvalid(true)
        showError(BOLETA_ERROR)
      } else {
        showError(errorMessage(cause))
      }
      setIsSubmitting(false)
    }
  }

  return (
    <div className="auth-page">
      <title>Iniciar sesión · MiESCOM</title>

      <form className="card card--accent form" onSubmit={handleSubmit} noValidate>
        <div className="card__header">
          <h1>Iniciar sesión</h1>
          <p className="muted">Registro de actividades culturales y deportivas</p>
        </div>

        {message && <Alert notice={message} />}

        <div className="field">
          <label htmlFor="boleta">Boleta</label>
          <input
            ref={boletaRef}
            id="boleta"
            name="boleta"
            inputMode="numeric"
            autoComplete="username"
            placeholder={BOLETA_EXAMPLE}
            required
            autoFocus
            aria-invalid={isBoletaInvalid}
            value={boleta}
            onChange={(event) => {
              setBoleta(event.target.value.replace(/\D/g, '').slice(0, BOLETA_LENGTH))
              setIsBoletaInvalid(false)
            }}
          />
        </div>

        <div className="field">
          <label htmlFor="password">Contraseña</label>
          <input
            ref={passwordRef}
            id="password"
            name="password"
            type="password"
            autoComplete="current-password"
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </div>

        <button className="button button--primary" type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Entrando…' : 'Entrar'}
        </button>

        <div className="card__footer">
          <p>¿Es tu primera vez en MiESCOM?</p>
          <Link className="button button--secondary" to="/registro">
            Registrarme
          </Link>
        </div>
      </form>
    </div>
  )
}
