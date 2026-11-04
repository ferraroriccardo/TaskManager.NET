using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

// SERVICE LAYER: contains the business logic. It knows nothing about HTTP (no status codes here):
// it receives DTOs, works with entities through the DbContext, returns DTOs.
public class TaskService : ITaskService
{
    private readonly AppDbContext _db;

    // Constructor injection: the DI container creates AppDbContext (Scoped) and passes it here.
    public TaskService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TaskDto>> GetAllAsync(int? projectId, TaskItemStatus? status, CancellationToken ct = default)
    {
        // IQueryable: the query is NOT executed yet, we are just "composing" it.
        IQueryable<TaskItem> query = _db.Tasks.AsNoTracking(); // read-only -> no change tracking, faster

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        // Projection with Select: EF generates A SINGLE SQL query with a JOIN on Projects
        // and reads only the columns it needs. Avoids the N+1 problem.
        // It is executed here, on ToListAsync().
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
            .Include(t => t.Project)          // Eager loading: also loads the project (JOIN)
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"Task {id} not found");

        return ToDto(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken ct = default)
    {
        // Business rule: the project must exist.
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

        _db.Tasks.Add(task);              // "Added" state in the change tracker (no query yet)
        await _db.SaveChangesAsync(ct);   // now the INSERT runs; EF populates task.Id

        return ToDto(task);
    }

    public async Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto, CancellationToken ct = default)
    {
        // Here we do NOT use AsNoTracking: we want EF to track the entity so we can modify it.
        var task = await _db.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException($"Task {id} not found");

        task.Title = dto.Title.Trim();
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.DueDate = ToUtc(dto.DueDate);

        // EF compares the values with the original ones and generates an UPDATE of only the changed fields.
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

    // PostgreSQL stores dates as "timestamp with time zone" and Npgsql only accepts DateTime in UTC
    // (otherwise it throws an exception). Here we normalize every date coming from the client.
    private static DateTime? ToUtc(DateTime? date) => date?.ToUniversalTime();

    // Entity -> DTO mapping. In large projects AutoMapper/Mapster is sometimes used,
    // but manual mapping is more explicit and easier to debug.
    private static TaskDto ToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority,
        t.DueDate, t.CreatedAt, t.ProjectId, t.Project.Name);
}
