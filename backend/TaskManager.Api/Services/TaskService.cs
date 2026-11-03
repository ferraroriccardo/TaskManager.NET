using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

// SERVICE LAYER: contiene la business logic. Non sa nulla di HTTP (niente status code qui):
// riceve DTO, lavora con le entità tramite il DbContext, restituisce DTO.
public class TaskService : ITaskService
{
    private readonly AppDbContext _db;

    // Constructor injection: il container DI crea AppDbContext (Scoped) e lo passa qui.
    public TaskService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TaskDto>> GetAllAsync(int? projectId, TaskItemStatus? status, CancellationToken ct = default)
    {
        // IQueryable: la query NON viene ancora eseguita, la stiamo solo "componendo".
        IQueryable<TaskItem> query = _db.Tasks.AsNoTracking(); // sola lettura -> niente change tracking, più veloce

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        // Projection con Select: EF genera UN'UNICA query SQL con JOIN su Projects
        // e legge solo le colonne che servono. Evita il problema N+1.
        // Viene eseguita qui, su ToListAsync().
        return await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Select(t => new TaskDto(
                t.Id, t.Title, t.Description, t.Status, t.Priority,
                t.DueDate, t.CreatedAt, t.ProjectId, t.Project.Name))
            .ToListAsync(ct);
    }

    public async Task<TaskDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var task = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.Project)          // Eager loading: carica anche il progetto (JOIN)
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"Task {id} not found");

        return ToDto(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken ct = default)
    {
        // Business rule: il progetto deve esistere.
        var project = await _db.Projects.FindAsync(new object[] { dto.ProjectId }, ct)
            ?? throw new NotFoundException($"Project {dto.ProjectId} not found");

        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Priority = dto.Priority,
            DueDate = ToUtc(dto.DueDate),
            ProjectId = project.Id,
            Project = project
        };

        _db.Tasks.Add(task);              // stato "Added" nel change tracker (ancora nessuna query)
        await _db.SaveChangesAsync(ct);   // ora parte l'INSERT; EF valorizza task.Id

        return ToDto(task);
    }

    public async Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto, CancellationToken ct = default)
    {
        // Qui NON usiamo AsNoTracking: vogliamo che EF tracci l'entità per poterla modificare.
        var task = await _db.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"Task {id} not found");

        task.Title = dto.Title.Trim();
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.DueDate = ToUtc(dto.DueDate);

        // EF confronta i valori con quelli originali e genera un UPDATE solo dei campi cambiati.
        await _db.SaveChangesAsync(ct);
        return ToDto(task);
    }

    public async Task<TaskDto> UpdateStatusAsync(int id, TaskItemStatus status, CancellationToken ct = default)
    {
        var task = await _db.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"Task {id} not found");

        task.Status = status;
        await _db.SaveChangesAsync(ct);
        return ToDto(task);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var task = await _db.Tasks.FindAsync(new object[] { id }, ct)
            ?? throw new NotFoundException($"Task {id} not found");

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(ct);
    }

    // PostgreSQL salva le date come "timestamp with time zone" e Npgsql accetta solo DateTime in UTC
    // (altrimenti lancia un'eccezione). Normalizziamo qui ogni data che arriva dal client.
    private static DateTime? ToUtc(DateTime? date) => date?.ToUniversalTime();

    // Mapping entità -> DTO. In progetti grandi si usa a volte AutoMapper/Mapster,
    // ma il mapping manuale è più esplicito e facile da debuggare.
    private static TaskDto ToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority,
        t.DueDate, t.CreatedAt, t.ProjectId, t.Project.Name);
}
