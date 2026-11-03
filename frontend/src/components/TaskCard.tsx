import { STATUSES, type Task, type TaskStatus } from '../types/task'

interface Props {
  task: Task
  onMove: (task: Task, status: TaskStatus) => void
  onDelete: (id: number) => void
}

export function TaskCard({ task, onMove, onDelete }: Props) {
  const index = STATUSES.indexOf(task.status)
  const prev = STATUSES[index - 1]
  const next = STATUSES[index + 1]

  return (
    <article className={`card priority-${task.priority.toLowerCase()}`}>
      <header>
        <span className="badge">{task.priority}</span>
        <button className="icon-btn" title="Delete" onClick={() => onDelete(task.id)}>
          ×
        </button>
      </header>
      <h4>{task.title}</h4>
      {task.description && <p className="desc">{task.description}</p>}
      {task.dueDate && (
        <p className="due">Due {new Date(task.dueDate).toLocaleDateString()}</p>
      )}
      <footer>
        {/* Rendering condizionale: il pulsante esiste solo se c'è una colonna precedente/successiva */}
        {prev ? <button onClick={() => onMove(task, prev)}>← Back</button> : <span />}
        {next && <button onClick={() => onMove(task, next)}>Next →</button>}
      </footer>
    </article>
  )
}
