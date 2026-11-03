import { useCallback, useEffect, useState } from 'react'
import { tasksApi } from '../api/tasksApi'
import type { CreateTaskRequest, Task, TaskStatus } from '../types/task'

// CUSTOM HOOK: incapsula stato + chiamate API dei task.
// I componenti non sanno nulla di fetch: chiamano createTask(), moveTask()... e ricevono i dati.
export function useTasks(projectId: number | null) {
  const [tasks, setTasks] = useState<Task[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // useEffect: "sincronizza" il componente con il server ogni volta che cambia projectId.
  useEffect(() => {
    if (projectId === null) {
      setTasks([])
      return
    }

    // AbortController: se l'utente cambia progetto prima che la risposta arrivi,
    // annulliamo la richiesta vecchia. Evita la race condition in cui una risposta
    // lenta del progetto A sovrascrive quella del progetto B.
    const controller = new AbortController()
    setLoading(true)
    setError(null)

    tasksApi
      .getByProject(projectId, controller.signal)
      .then(setTasks)
      .catch((err: unknown) => {
        if (controller.signal.aborted) return // annullata da noi: non è un errore
        setError(err instanceof Error ? err.message : 'Unknown error')
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false)
      })

    // Cleanup: eseguita quando projectId cambia o il componente viene smontato.
    return () => controller.abort()
  }, [projectId])

  // useCallback: la funzione mantiene la stessa identità tra un render e l'altro
  // (utile se la passi a componenti figli memoizzati).
  const createTask = useCallback(async (data: CreateTaskRequest) => {
    const created = await tasksApi.create(data)
    setTasks((prev) => [...prev, created]) // aggiornamento IMMUTABILE: nuovo array, mai push()
  }, [])

  // OPTIMISTIC UPDATE: aggiorno subito la UI, poi chiamo il server.
  // Se il server fallisce, torno allo stato precedente (rollback).
  const moveTask = useCallback(async (task: Task, status: TaskStatus) => {
    const setStatus = (s: TaskStatus) =>
      setTasks((prev) => prev.map((t) => (t.id === task.id ? { ...t, status: s } : t)))

    setStatus(status) // la card si sposta subito
    try {
      await tasksApi.updateStatus(task.id, status)
    } catch (err) {
      setStatus(task.status) // rollback allo stato originale
      setError(err instanceof Error ? err.message : 'Could not move task')
    }
  }, [])

  const deleteTask = useCallback(async (id: number) => {
    await tasksApi.remove(id)
    setTasks((prev) => prev.filter((t) => t.id !== id))
  }, [])

  return { tasks, loading, error, createTask, moveTask, deleteTask }
}
