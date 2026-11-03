import { useState } from 'react'
import { ProjectSidebar } from './components/ProjectSidebar'
import { TaskBoard } from './components/TaskBoard'
import { useProjects } from './hooks/useProjects'

// Componente radice: tiene lo stato condiviso tra sidebar e board
// ("lifting state up": lo stato sta nel primo antenato comune dei componenti che lo usano).
export default function App() {
  const { projects, error, reload, createProject, deleteProject } = useProjects()
  const [selectedId, setSelectedId] = useState<number | null>(null)

  // Se non è selezionato nessun progetto (o quello selezionato è stato eliminato), usa il primo.
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
