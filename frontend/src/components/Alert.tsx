export type Notice = {
  tone: 'error' | 'info' | 'success'
  message: string
}

export function Alert({ notice }: { notice: Notice }) {
  return (
    <p className={`alert alert--${notice.tone}`} role={notice.tone === 'error' ? 'alert' : 'status'}>
      {notice.message}
    </p>
  )
}
