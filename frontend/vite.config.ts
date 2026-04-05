import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // Автоматически выбираем свободный порт и сразу открываем браузер
    open: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5189',
        changeOrigin: true,
        secure: false,
      },
      '/avatars': {
        target: 'http://localhost:5189',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
