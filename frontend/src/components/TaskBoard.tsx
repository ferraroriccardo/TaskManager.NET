import { useTasks } from '../hooks/useTasks'
import { STATUSES, type CreateTaskRequest, type TaskStatus } from '../types/task'
import { TaskCard } from './TaskCard'
import { TaskForm } from './TaskForm'

const COLUMN_TITLES: Record<TaskStatus, string> = {
  Todo: 'To do',
  InProgress: 'In progress',
  Done: 'Done',
}

interface Props {
  projectId: number
  projectName: string
  onTasksChanged: () => void // per aggiornare il contatore nella sidebar
}

export function TaskBoard({ projectId, projectName, onTasksChanged }: Props) {
  const { tasks, loading, error, createTask, moveTask, deleteTask } = useTasks(projectId)

  async function handleCreate(data: CreateTaskRequest) {
    await createTask(data)
    onTasksChanged()
  }

  async function handleDelete(id: number) {
    await deleteTask(id)
    onTasksChanged()
  }

  return (
    <main className="board">
      <h1>{projectName}</h1>
      <TaskForm projectId={projectId} onCreate={handleCreate} />

      {error && <p className="error">{error}</p>}
      {loading && <p className="muted">Loading…</p>}

      <div className="columns">
        {STATUSES.map((status) => {
          // "Stato derivato": non salviamo le tre liste nello state, le calcoliamo a ogni render
          // filtrando l'unica fonte di verità (tasks).
          const columnTasks = tasks.filter((t) => t.status === status)
          return (
            <section key={status} className="column">
              <h3>
                {COLUMN_TITLES[status]} <span className="count">{columnTasks.length}</span>
              </h3>
              {columnTasks.map((task) => (
                <TaskCard key={task.id} task={task} onMove={moveTask} onDelete={handleDelete} />
              ))}
              {columnTasks.length === 0 && <p className="muted empty">Nothing here</p>}
            </section>
          )
        })}
      </div>
    </main>
  )
}
