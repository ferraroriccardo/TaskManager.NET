import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [react()],
  // Porta fissa: deve coincidere con "Cors:AllowedOrigins" in appsettings.json del backend
  server: { port: 5173, strictPort: true },
})
