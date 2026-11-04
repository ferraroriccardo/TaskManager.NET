import { useState } from 'react'
import { ProjectSidebar } from './components/ProjectSidebar'
import { TaskBoard } from './components/TaskBoard'
import { useProjects } from './hooks/useProjects'

// Root component: holds the state shared between sidebar and board
// ("lifting state up": the state lives in the closest common ancestor of the components that use it).
export default function App() {
  const { projects, error, reload, createProject, deleteProject } = useProjects()
  const [selectedId, setSelectedId] = useState<number | null>(null)

  // If no project is selected (or the selected one was deleted), use the first one.
  const selected = projects.find((p) => p.id === selectedId) ?? projects[0] ?? null

  async function handleCreate(name: string) {
    const created = await createProject(name)
    setSelectedId(created.id)
  }

  async function handleDelete(id: number) {
    if (!window.confirm('Delete this project and all its tasks?')) return
    await deleteProject(id)
  }

  return (
    <div className="layout">
      <ProjectSidebar
        projects={projects}
        selectedId={selected?.id ?? null}
        onSelect={setSelectedId}
        onCreate={handleCreate}
        onDelete={handleDelete}
      />

      {error && (
        <main className="board">
          <p className="error">
            Cannot reach the API ({error}). Is the backend running on http://localhost:5080?
          </p>
        </main>
      )}

      {!error && selected && (
        <TaskBoard
          projectId={selected.id}
          projectName={selected.name}
          onTasksChanged={() => reload()}
        />
      )}

      {!error && !selected && (
        <main className="board">
          <p className="muted">Create a project to get started.</p>
        </main>
      )}
    </div>
  )
}
