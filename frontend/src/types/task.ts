// Questi tipi sono lo "specchio" TypeScript dei DTO C# (backend/TaskManager.Api/DTOs).
// Se nel backend cambi un DTO, devi aggiornare anche qui: TypeScript ti segnalerà
// a compile-time ogni punto del frontend che si rompe.

// Union type al posto dell'enum C#: il backend serializza gli enum come stringhe.
export type TaskStatus = 'Todo' | 'InProgress' | 'Done'
export type TaskPriority = 'Low' | 'Medium' | 'High'

export const STATUSES: TaskStatus[] = ['Todo', 'InProgress', 'Done']
export const PRIORITIES: TaskPriority[] = ['Low', 'Medium', 'High']

// <-> TaskDto.cs
export interface Task {
  id: number
  title: string
  description: string | null
  status: TaskStatus
  priority: TaskPriority
  dueDate: string | null // le date arrivano come stringhe ISO 8601 nel JSON
  createdAt: string
  projectId: number
  projectName: string
}

// <-> CreateTaskDto.cs
export interface CreateTaskRequest {
  title: string
  description: string | null
  priority: TaskPriority
  dueDate: string | null
  projectId: number
}

// <-> ProjectDto.cs
export interface Project {
  id: number
  name: string
  taskCount: number
}

// Formato degli errori restituiti da ASP.NET Core (ProblemDetails)
export interface ProblemDetails {
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]> // presente negli errori di validazione (400)
}
