namespace TaskManager.Api.Models;

// The three columns of the Kanban board.
public enum TaskItemStatus
{
    Todo,
    InProgress,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}
