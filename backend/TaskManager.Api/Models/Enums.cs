namespace TaskManager.Api.Models;

// Le tre colonne della board Kanban.
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
