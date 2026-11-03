import { useCallback, useEffect, useState } from 'react'
import { projectsApi } from '../api/projectsApi'
import type { Project } from '../types/task'

export function useProjects() {
  const [projects, setProjects] = useState<Project[]>([])
  const [error, setError] = useState<string | null>(null)

  const reload = useCallback(async (signal?: AbortSignal) => {
    try {
      setProjects(await projectsApi.getAll(signal))
      setError(null)
    } catch (err) {
      if (signal?.aborted) return
      setError(err instanceof Error ? err.message : 'Unknown error')
    }
  }, [])

  // Array di dipendenze [reload]: reload non cambia mai (useCallback con []),
  // quindi l'effetto parte una sola volta, al primo render.
  useEffect(() => {
    const controller = new AbortController()
    reload(controller.signal)
    return () => controller.abort()
  }, [reload])

  const createProject = useCallback(async (name: string) => {
    const created = await projectsApi.create(name)
    setProjects((prev) => [...prev, created].sort((a, b) => a.name.localeCompare(b.name)))
    return created
  }, [])

  const deleteProject = useCallback(async (id: number) => {
    await projectsApi.remove(id)
    setProjects((prev) => prev.filter((p) => p.id !== id))
  }, [])

  return { projects, error, reload, createProject, deleteProject }
}
