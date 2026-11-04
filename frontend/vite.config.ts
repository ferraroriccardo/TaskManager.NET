import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [react()],
  // Fixed port: must match "Cors:AllowedOrigins" in the backend's appsettings.json
  server: { port: 5173, strictPort: true },
})
