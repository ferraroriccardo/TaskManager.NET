namespace TaskManager.Api.Models;

// ENTITÀ: un progetto contiene molti task (relazione 1-N).
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation property "collection": tutti i task di questo progetto.
    public List<TaskItem> Tasks { get; set; } = new();
}
