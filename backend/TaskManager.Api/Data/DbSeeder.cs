using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

// Inserisce qualche dato di esempio al primo avvio, così la UI non è vuota.
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Projects.Any()) return; // già popolato

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
        db.SaveChanges(); // un'unica transazione: INSERT dei progetti + INSERT dei task
    }
}
