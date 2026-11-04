using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

// Inserts some sample data on first startup, so the UI isn't empty.
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Projects.Any()) return; // already populated

        var interview = new Project
        {
            Name = "Deloitte interview prep",
            Tasks =
            {
                new TaskItem { Title = "Study Dependency Injection lifetimes", Priority = TaskPriority.High, Status = TaskItemStatus.Done },
                new TaskItem { Title = "Understand EF Core change tracking", Priority = TaskPriority.High, Status = TaskItemStatus.InProgress },
                new TaskItem { Title = "Trace a request from React to SQL", Description = "Follow the GUIDA_STUDIO.md walkthrough", Priority = TaskPriority.Medium },
                new TaskItem { Title = "Prepare 'Why Deloitte, why Luxembourg?'", Priority = TaskPriority.Medium, DueDate = DateTime.UtcNow.AddDays(2) }
            }
        };

        var personal = new Project
        {
            Name = "Personal",
            Tasks =
            {
                new TaskItem { Title = "Book train to Luxembourg", Priority = TaskPriority.Low }
            }
        };

        db.Projects.AddRange(interview, personal);
        db.SaveChanges(); // a single transaction: INSERT of the projects + INSERT of the tasks
    }
}
