import { useState, type FormEvent } from 'react'
import type { Project } from '../types/task'

// "Props" are the component's parameters: input data + callbacks to communicate upwards.
// Data flows down (parent -> child), events flow up (child -> parent via callbacks).
interface Props {
  projects: Project[]
  selectedId: number | null
  onSelect: (id: number) => void
  onCreate: (name: string) => Promise<void>
  onDelete: (id: number) => Promise<void>
}

export function ProjectSidebar({ projects, selectedId, onSelect, onCreate, onDelete }: Props) {
  // LOCAL state: only this component cares about it (the text in the input).
  // It's a "controlled input": the displayed value always comes from React state.
  const [name, setName] = useState('')

  async function handleSubmit(e: FormEvent) {
    e.preventDefault() // prevents the page refresh (default behavior of HTML forms)
    if (!name.trim()) return
    await onCreate(name.trim())
    setName('')
  }

  return (
    <aside className="sidebar">
      <h2>Projects</h2>
      <ul>
        {/* "key" helps React figure out which list item changed during reconciliation */}
        {projects.map((p) => (
          <li key={p.id} className={p.id === selectedId ? 'active' : ''}>
            <button className="project-name" onClick={() => onSelect(p.id)}>
              {p.name} <span className="count">{p.taskCount}</span>
            </button>
            <button
              className="icon-btn"
              title="Delete project"
              onClick={() => onDelete(p.id)}
            >
              ×
            </button>
          </li>
        ))}
      </ul>

      <form onSubmit={handleSubmit} className="inline-form">
        <input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="New project…"
          maxLength={100}
        />
        <button type="submit">Add</button>
      </form>
    </aside>
  )
}
