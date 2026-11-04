namespace TaskManager.Api.Models;

// ENTITY: represents a row of the "Tasks" table in the database.
// It's called TaskItem (not Task) to avoid confusion with System.Threading.Tasks.Task.
public class TaskItem
{
    public int Id { get; set; }                       // Primary Key (by EF convention: "Id")
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }          // "?" = can be NULL in the DB
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // N-1 relationship: each task belongs to ONE project.
    public int ProjectId { get; set; }                // Foreign Key
    public Project Project { get; set; } = null!;     // Navigation property
}
