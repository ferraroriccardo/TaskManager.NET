import type { Project } from '../types/task'
import { request } from './client'

// One method per endpoint of ProjectsController.cs
export const projectsApi = {
  getAll: (signal?: AbortSignal) => request<Project[]>('/api/projects', { signal }),

  create: (name: string) =>
    request<Project>('/api/projects', { method: 'POST', body: JSON.stringify({ name }) }),

  remove: (id: number) => request<void>(`/api/projects/${id}`, { method: 'DELETE' }),
}
