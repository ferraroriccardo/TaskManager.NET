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
        // p.Tasks.Count() dentro il Select diventa una subquery COUNT(*) in SQL:
        // NON carica tutti i task in memoria. Confrontalo con la versione "sbagliata"
        // descritta nella guida (N+1 / caricare tutto e contare in C#).
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

        _db.Projects.Remove(project); // i task vengono eliminati in cascata (vedi AppDbContext)
        await _db.SaveChangesAsync(ct);
    }
}
