import { useRef, useState, type FormEvent } from 'react'
import { Link } from 'react-router'
import { BOLETA_LENGTH, InvalidBoletaError, registrar } from '../api/auth'
import { errorMessage } from '../api/http'
import { Alert, type Notice } from '../components/Alert'

const BOLETA_ERROR = `La boleta debe tener ${BOLETA_LENGTH} dígitos.`

/**
 * Registro de usuario: valida que la boleta esté en la tabla de alumnos y envía una
 * contraseña temporal al correo registrado. Al entrar con ella se pide cambiarla.
 */
export function RegistroPage() {
  const [boleta, setBoleta] = useState('')
  const [message, setMessage] = useState<Notice>()
  const [isBoletaInvalid, setIsBoletaInvalid] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [sentTo, setSentTo] = useState<string>()
  const boletaRef = useRef<HTMLInputElement>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (boleta.length !== BOLETA_LENGTH) {
      setIsBoletaInvalid(true)
      setMessage({ tone: 'error', message: BOLETA_ERROR })
      boletaRef.current?.focus()
      return
    }

    setIsBoletaInvalid(false)
    setMessage(undefined)
    setIsSubmitting(true)

    try {
      setSentTo(await registrar(boleta))
    } catch (cause) {
      if (cause instanceof InvalidBoletaError) {
        setIsBoletaInvalid(true)
        setMessage({ tone: 'error', message: BOLETA_ERROR })
      } else {
        setMessage({ tone: 'error', message: errorMessage(cause) })
      }
      boletaRef.current?.focus()
    } finally {
      setIsSubmitting(false)
    }
  }

  if (sentTo) {
    return (
      <div className="auth-page">
        <title>Revisa tu correo · MiESCOM</title>
        <section className="card card--accent form" aria-labelledby="registro-listo">
          <div className="card__header">
            <h1 id="registro-listo">Revisa tu correo</h1>
          </div>
          <Alert notice={{ tone: 'success', message: `Enviamos tu contraseña temporal a ${sentTo}.` }} />
          <p>
            Entra con tu boleta y esa contraseña. Al hacerlo te pediremos cambiarla por una propia.
          </p>
          <p className="muted">Si no la ves en unos minutos, revisa la carpeta de correo no deseado.</p>
          <Link className="button button--primary" to="/">
            Ir a iniciar sesión
          </Link>
        </section>
      </div>
    )
  }

  return (
    <div className="auth-page">
      <title>Registrarme · MiESCOM</title>

      <form className="card card--accent form" onSubmit={handleSubmit} noValidate>
        <div className="card__header">
          <h1>Registrarme</h1>
          <p className="muted">
            Escribe tu boleta. Si estás inscrito en ESCOM, te enviaremos una contraseña temporal a tu
            correo registrado.
          </p>
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

        <button className="button button--primary" type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Enviando…' : 'Enviar contraseña temporal'}
        </button>

        <div className="card__footer">
          <p>¿Ya tienes cuenta?</p>
          <Link className="button button--secondary" to="/">
            Iniciar sesión
          </Link>
        </div>
      </form>
    </div>
  )
}
