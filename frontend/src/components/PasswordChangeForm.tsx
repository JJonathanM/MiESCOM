import { useRef, useState, type FormEvent } from 'react'
import { cambiarPassword, PASSWORD_MIN_LENGTH, type AuthSession } from '../api/auth'
import { ApiError, errorMessage } from '../api/http'
import { Alert, type Notice } from './Alert'

type Field = 'passwordActual' | 'passwordNueva' | 'confirmacion'

type PasswordChangeFormProps = {
  boleta: string
  /** La contraseña actual es la temporal que llegó por correo. */
  isTemporary: boolean
  onChanged: (session: AuthSession) => void
}

/** Formulario para cambiar la contraseña: actual (o temporal), nueva y confirmación. */
export function PasswordChangeForm({ boleta, isTemporary, onChanged }: PasswordChangeFormProps) {
  const [values, setValues] = useState<Record<Field, string>>({
    passwordActual: '',
    passwordNueva: '',
    confirmacion: '',
  })
  const [message, setMessage] = useState<Notice>()
  const [invalidField, setInvalidField] = useState<Field>()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const refs = {
    passwordActual: useRef<HTMLInputElement>(null),
    passwordNueva: useRef<HTMLInputElement>(null),
    confirmacion: useRef<HTMLInputElement>(null),
  }

  function fail(field: Field, text: string) {
    setInvalidField(field)
    setMessage({ tone: 'error', message: text })
    refs[field].current?.focus()
  }

  function validate(): boolean {
    const { passwordActual, passwordNueva, confirmacion } = values
    if (passwordActual === '') {
      fail('passwordActual', isTemporary ? 'Escribe la contraseña temporal que recibiste.' : 'Escribe tu contraseña actual.')
      return false
    }
    if (passwordNueva.length < PASSWORD_MIN_LENGTH) {
      fail('passwordNueva', `La nueva contraseña debe tener al menos ${PASSWORD_MIN_LENGTH} caracteres.`)
      return false
    }
    if (passwordNueva.includes(boleta)) {
      fail('passwordNueva', 'La nueva contraseña no puede contener tu boleta.')
      return false
    }
    if (passwordNueva === passwordActual) {
      fail('passwordNueva', 'La nueva contraseña debe ser distinta de la actual.')
      return false
    }
    if (confirmacion !== passwordNueva) {
      fail('confirmacion', 'La confirmación no coincide con la nueva contraseña.')
      return false
    }
    return true
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!validate()) {
      return
    }

    setInvalidField(undefined)
    setMessage(undefined)
    setIsSubmitting(true)

    try {
      const session = await cambiarPassword(values.passwordActual, values.passwordNueva)
      setValues({ passwordActual: '', passwordNueva: '', confirmacion: '' })
      setIsSubmitting(false)
      setMessage({ tone: 'success', message: 'Tu contraseña se actualizó.' })
      onChanged(session)
    } catch (cause) {
      setIsSubmitting(false)
      const field = cause instanceof ApiError ? (cause.field as Field | undefined) : undefined
      if (field && field in refs) {
        fail(field, cause instanceof Error ? cause.message : '')
      } else {
        setMessage({ tone: 'error', message: errorMessage(cause) })
      }
    }
  }

  function input(field: Field, label: string, autoComplete: string) {
    return (
      <div className="field">
        <label htmlFor={field}>{label}</label>
        <input
          ref={refs[field]}
          id={field}
          name={field}
          type="password"
          autoComplete={autoComplete}
          required
          aria-invalid={invalidField === field}
          value={values[field]}
          onChange={(event) => {
            setValues((current) => ({ ...current, [field]: event.target.value }))
            if (invalidField === field) {
              setInvalidField(undefined)
            }
          }}
        />
      </div>
    )
  }

  return (
    <form className="form" onSubmit={handleSubmit} noValidate>
      {message && <Alert notice={message} />}

      {input('passwordActual', isTemporary ? 'Contraseña temporal' : 'Contraseña actual', 'current-password')}
      {input('passwordNueva', 'Nueva contraseña', 'new-password')}
      {input('confirmacion', 'Confirma la nueva contraseña', 'new-password')}

      <button className="button button--primary" type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Guardando…' : 'Cambiar contraseña'}
      </button>
    </form>
  )
}
