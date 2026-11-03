using TaskManager.Api.DTOs;

namespace TaskManager.Api.Services;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProjectDto> CreateAsync(CreateProjectDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
