import type { ProblemDetails } from '../types/task'

// Backend URL, read from the .env file (variables exposed by Vite must start with VITE_)
const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080'

// "Typed" error that keeps the backend's status code and message.
export class ApiError extends Error {
  status: number
  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

// Generic wrapper around fetch: a single place to handle
// base URL, JSON headers, response parsing and HTTP errors.
// (Later on you could add the "Authorization: Bearer <token>" header here)
export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: { 'Content-Type': 'application/json', ...options.headers },
  })

  // fetch does NOT throw for 4xx/5xx: response.ok must be checked manually.
  if (!response.ok) {
    let message = `HTTP ${response.status}`
    try {
      const problem = (await response.json()) as ProblemDetails
      // Validation errors: { errors: { Title: ["The Title field is required."] } }
      const firstValidationError = problem.errors ? Object.values(problem.errors)[0]?.[0] : undefined
      message = firstValidationError ?? problem.detail ?? problem.title ?? message
    } catch {
      // response without a JSON body
    }
    throw new ApiError(response.status, message)
  }

  // 204 No Content (e.g. DELETE): no body to read
  if (response.status === 204) return undefined as T

  return (await response.json()) as T
}
