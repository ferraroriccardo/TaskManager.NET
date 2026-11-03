import type { ProblemDetails } from '../types/task'

// URL del backend, letto dal file .env (le variabili esposte da Vite devono iniziare con VITE_)
const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080'

// Errore "tipizzato" che conserva status code e messaggio del backend.
export class ApiError extends Error {
  status: number
  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

// Wrapper generico attorno a fetch: un unico punto dove gestire
// URL base, header JSON, parsing della risposta ed errori HTTP.
// (Più avanti qui potresti aggiungere l'header "Authorization: Bearer <token>")
export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: { 'Content-Type': 'application/json', ...options.headers },
  })

  // fetch NON lancia eccezioni per 4xx/5xx: va controllato response.ok a mano.
  if (!response.ok) {
    let message = `HTTP ${response.status}`
    try {
      const problem = (await response.json()) as ProblemDetails
      // Errori di validazione: { errors: { Title: ["The Title field is required."] } }
      const firstValidationError = problem.errors ? Object.values(problem.errors)[0]?.[0] : undefined
      message = firstValidationError ?? problem.detail ?? problem.title ?? message
    } catch {
      // risposta senza body JSON
    }
    throw new ApiError(response.status, message)
  }

  // 204 No Content (es. DELETE): nessun body da leggere
  if (response.status === 204) return undefined as T

  return (await response.json()) as T
}
