export type Notice = {
  tone: 'error' | 'info'
  message: string
}

export function Alert({ notice }: { notice: Notice }) {
  return (
    <p className={`alert alert--${notice.tone}`} role="alert">
      {notice.message}
    </p>
  )
}
