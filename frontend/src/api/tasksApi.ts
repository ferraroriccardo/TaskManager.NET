import type { CreateTaskRequest, Task, TaskStatus } from '../types/task'
import { request } from './client'

// Un metodo per ogni endpoint di TasksController.cs
export const tasksApi = {
  getByProject: (projectId: number, signal?: AbortSignal) =>
    request<Task[]>(`/api/tasks?projectId=${projectId}`, { signal }),

  create: (data: CreateTaskRequest) =>
    request<Task>('/api/tasks', { method: 'POST', body: JSON.stringify(data) }),

  updateStatus: (id: number, status: TaskStatus) =>
    request<Task>(`/api/tasks/${id}/status`, { method: 'PATCH', body: JSON.stringify({ status }) }),

  remove: (id: number) => request<void>(`/api/tasks/${id}`, { method: 'DELETE' }),
}
