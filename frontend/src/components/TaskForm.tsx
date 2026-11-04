import { useState, type FormEvent } from 'react'
import { PRIORITIES, type CreateTaskRequest, type TaskPriority } from '../types/task'

interface Props {
  projectId: number
  onCreate: (data: CreateTaskRequest) => Promise<void>
}

export function TaskForm({ projectId, onCreate }: Props) {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState<TaskPriority>('Medium')
  const [dueDate, setDueDate] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setError(null)
    try {
      await onCreate({
        title,
        description: description || null,
        priority,
        // <input type="date"> gives "2026-11-05": we convert it to ISO 8601 for the backend
        dueDate: dueDate ? new Date(dueDate).toISOString() : null,
        projectId,
      })
      setTitle('')
      setDescription('')
      setDueDate('')
    } catch (err) {
      // Backend validation errors (400) end up here too — try removing
      // the "required" attribute from the input and submit an empty title.
      setError(err instanceof Error ? err.message : 'Error')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form className="task-form" onSubmit={handleSubmit}>
      <input
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        placeholder="What needs to be done?"
        maxLength={200}
        required
      />
      <input
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        placeholder="Description (optional)"
      />
      <select value={priority} onChange={(e) => setPriority(e.target.value as TaskPriority)}>
        {PRIORITIES.map((p) => (
          <option key={p} value={p}>
            {p}
          </option>
        ))}
      </select>
      <input type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} />
      <button type="submit" disabled={submitting}>
        {submitting ? 'Adding…' : 'Add task'}
      </button>
      {error && <p className="error">{error}</p>}
    </form>
  )
}
