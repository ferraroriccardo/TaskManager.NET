namespace TaskManager.Api.Models;

// ENTITÀ: rappresenta una riga della tabella "Tasks" nel database.
// Si chiama TaskItem (e non Task) per non confondersi con System.Threading.Tasks.Task.
public class TaskItem
{
    public int Id { get; set; }                       // Primary Key (per convenzione EF: "Id")
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }          // "?" = può essere NULL nel DB
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relazione N-1: ogni task appartiene a UN progetto.
    public int ProjectId { get; set; }                // Foreign Key
    public Project Project { get; set; } = null!;     // Navigation property
}
