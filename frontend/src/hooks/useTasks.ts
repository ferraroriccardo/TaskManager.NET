import { useCallback, useEffect, useState } from 'react'
import { tasksApi } from '../api/tasksApi'
import type { CreateTaskRequest, Task, TaskStatus } from '../types/task'

// CUSTOM HOOK: encapsulates the tasks' state + API calls.
// Components know nothing about fetch: they call createTask(), moveTask()... and receive the data.
export function useTasks(projectId: number | null) {
  const [tasks, setTasks] = useState<Task[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // useEffect: "synchronizes" the component with the server every time projectId changes.
  useEffect(() => {
    if (projectId === null) {
      setTasks([])
      return
    }

    // AbortController: if the user switches project before the response arrives,
    // we cancel the old request. This avoids the race condition where a slow response
    // for project A overwrites the one for project B.
    const controller = new AbortController()
    setLoading(true)
    setError(null)

    tasksApi
      .getByProject(projectId, controller.signal)
      .then(setTasks)
      .catch((err: unknown) => {
        if (controller.signal.aborted) return // cancelled by us: not an error
        setError(err instanceof Error ? err.message : 'Unknown error')
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false)
      })

    // Cleanup: runs when projectId changes or the component is unmounted.
    return () => controller.abort()
  }, [projectId])

  // useCallback: the function keeps the same identity from one render to the next
  // (useful if you pass it to memoized child components).
  const createTask = useCallback(async (data: CreateTaskRequest) => {
    const created = await tasksApi.create(data)
    setTasks((prev) => [...prev, created]) // IMMUTABLE update: new array, never push()
  }, [])

  // OPTIMISTIC UPDATE: update the UI immediately, then call the server.
  // If the server fails, go back to the previous state (rollback).
  const moveTask = useCallback(async (task: Task, status: TaskStatus) => {
    const setStatus = (s: TaskStatus) =>
      setTasks((prev) => prev.map((t) => (t.id === task.id ? { ...t, status: s } : t)))

    setStatus(status) // the card moves immediately
    try {
      await tasksApi.updateStatus(task.id, status)
    } catch (err) {
      setStatus(task.status) // rollback to the original status
      setError(err instanceof Error ? err.message : 'Could not move task')
    }
  }, [])

  const deleteTask = useCallback(async (id: number) => {
    await tasksApi.remove(id)
    setTasks((prev) => prev.filter((t) => t.id !== id))
  }, [])

  return { tasks, loading, error, createTask, moveTask, deleteTask }
}
