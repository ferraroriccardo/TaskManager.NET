namespace TaskManager.Api.Models;

// ENTITY: a project contains many tasks (1-N relationship).
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // "Collection" navigation property: all the tasks of this project.
    public List<TaskItem> Tasks { get; set; } = new();
}
