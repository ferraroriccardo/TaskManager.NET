using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

// Il DbContext è il "ponte" tra C# e il database:
//  - ogni DbSet<T> corrisponde a una tabella
//  - tiene traccia delle modifiche agli oggetti (change tracking)
//  - SaveChangesAsync() le traduce in INSERT/UPDATE/DELETE dentro una transazione (Unit of Work)
public class AppDbContext : DbContext
{
    // Le opzioni (quale DB, quale connection string) arrivano da Program.cs tramite Dependency Injection.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    // Fluent API: configurazione dello schema che non si può dedurre dalle convenzioni.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(p => p.Name)
                  .IsRequired()
                  .HasMaxLength(100);         // -> NVARCHAR(100) NOT NULL su SQL Server
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Title)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(t => t.Description)
                  .HasMaxLength(2000);

            // Salva lo stato come testo ("Todo") invece che come numero (0): DB più leggibile.
            entity.Property(t => t.Status).HasConversion<string>();
            // La priorità invece resta un intero (Low=0, Medium=1, High=2): così ORDER BY
            // ordina per importanza. Come stringa ordinerebbe alfabeticamente (High < Low < Medium)!

            // Relazione 1-N esplicita. Se elimini un progetto, si eliminano anche i suoi task.
            entity.HasOne(t => t.Project)
                  .WithMany(p => p.Tasks)
                  .HasForeignKey(t => t.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indice: velocizza la query più frequente ("dammi i task del progetto X").
            entity.HasIndex(t => t.ProjectId);
        });
    }
}
