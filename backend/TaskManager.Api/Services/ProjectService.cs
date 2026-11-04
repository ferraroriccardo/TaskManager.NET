using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProjectDto>> GetAllAsync(CancellationToken ct = default)
    {
        // p.Tasks.Count() inside the Select becomes a COUNT(*) subquery in SQL:
        // it does NOT load all the tasks into memory. Compare it with the "wrong" version
        // described in the guide (N+1 / loading everything and counting in C#).
        return await _db.Projects
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProjectDto(p.Id, p.Name, p.Tasks.Count()))
            .ToListAsync(ct);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto, CancellationToken ct = default)
    {
        var project = new Project { Name = dto.Name.Trim() };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);
        return new ProjectDto(project.Id, project.Name, 0);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await _db.Projects.FindAsync(new object[] { id }, ct)
            ?? throw new NotFoundException($"Project {id} not found");

        _db.Projects.Remove(project); // the tasks are cascade-deleted (see AppDbContext)
        await _db.SaveChangesAsync(ct);
    }
}
