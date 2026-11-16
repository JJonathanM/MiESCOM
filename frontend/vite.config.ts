import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    // El navegador solo habla con Vite, así que la cookie de sesión es del mismo
    // origen y no hace falta configurar CORS en el backend.
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
})
