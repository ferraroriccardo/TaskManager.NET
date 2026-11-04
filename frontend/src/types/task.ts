// These types are the TypeScript "mirror" of the C# DTOs (backend/TaskManager.Api/DTOs).
// If you change a DTO in the backend, you must update it here too: TypeScript will flag
// at compile time every place in the frontend that breaks.

// Union type instead of the C# enum: the backend serializes enums as strings.
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
  dueDate: string | null // dates arrive as ISO 8601 strings in the JSON
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

// Format of the errors returned by ASP.NET Core (ProblemDetails)
export interface ProblemDetails {
  title?: string
  status?: number
  detail?: string
  errors?: Record<string, string[]> // present in validation errors (400)
}
