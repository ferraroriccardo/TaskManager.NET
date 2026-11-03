import { useState, type FormEvent } from 'react'
import type { Project } from '../types/task'

// Le "props" sono i parametri del componente: dati in ingresso + callback per comunicare verso l'alto.
// I dati scendono (parent -> child), gli eventi salgono (child -> parent tramite callback).
interface Props {
  projects: Project[]
  selectedId: number | null
  onSelect: (id: number) => void
  onCreate: (name: string) => Promise<void>
  onDelete: (id: number) => Promise<void>
}

export function ProjectSidebar({ projects, selectedId, onSelect, onCreate, onDelete }: Props) {
  // Stato LOCALE: interessa solo a questo componente (il testo nell'input).
  // È un "controlled input": il valore mostrato viene sempre dallo stato React.
  const [name, setName] = useState('')

  async function handleSubmit(e: FormEvent) {
    e.preventDefault() // evita il refresh della pagina (comportamento di default dei form HTML)
    if (!name.trim()) return
    await onCreate(name.trim())
    setName('')
  }

  return (
    <aside className="sidebar">
      <h2>Projects</h2>
      <ul>
        {/* "key" aiuta React a capire quale elemento della lista è cambiato durante la reconciliation */}
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
