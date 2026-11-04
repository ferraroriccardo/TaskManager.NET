using TaskManager.Api.DTOs;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

// INTERFACE: defines WHAT the service can do, not HOW.
// The controller depends on the interface, not on the concrete class:
// so in tests you can pass a "mock" and in Program.cs you choose the implementation (Dependency Inversion).
public interface ITaskService
{
    Task<List<TaskDto>> GetAllAsync(int? projectId, TaskItemStatus? status, CancellationToken ct = default);
    Task<TaskDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken ct = default);
    Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto, CancellationToken ct = default);
    Task<TaskDto> UpdateStatusAsync(int id, TaskItemStatus status, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
