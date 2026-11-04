using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

// The DbContext is the "bridge" between C# and the database:
//  - each DbSet<T> maps to a table
//  - it keeps track of changes to objects (change tracking)
//  - SaveChangesAsync() translates them into INSERT/UPDATE/DELETE inside a transaction (Unit of Work)
public class AppDbContext : DbContext
{
    // The options (which DB, which connection string) come from Program.cs via Dependency Injection.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    // Fluent API: schema configuration that can't be inferred from conventions.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(p => p.Name)
                  .IsRequired()
                  .HasMaxLength(100);         // -> NVARCHAR(100) NOT NULL on SQL Server
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Title)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(t => t.Description)
                  .HasMaxLength(2000);

            // Stores the status as text ("Todo") instead of a number (0): a more readable DB.
            entity.Property(t => t.Status).HasConversion<string>();
            // Priority instead stays an integer (Low=0, Medium=1, High=2): this way ORDER BY
            // sorts by importance. As a string it would sort alphabetically (High < Low < Medium)!

            // Explicit 1-N relationship. If you delete a project, its tasks are deleted too.
            entity.HasOne(t => t.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index: speeds up the most frequent query ("give me the tasks of project X").
            entity.HasIndex(t => t.ProjectId);
        });
    }
}
