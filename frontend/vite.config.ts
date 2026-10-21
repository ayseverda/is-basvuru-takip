import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Backend'in adresi (Swagger'da gördüğün port)
const backend = {
  target: 'https://localhost:7202',
  secure: false, // Geliştirme sertifikasına güven
}

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Bu adreslerle başlayan istekler backend'e iletilir
      '/api': backend,
      '/login': backend,
      '/register': backend,
    },
  },
})