using TaskManager.Api.DTOs;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

// INTERFACCIA: definisce COSA può fare il servizio, non COME.
// Il controller dipende dall'interfaccia, non dalla classe concreta:
// così nei test puoi passare un "mock" e in Program.cs scegli l'implementazione (Dependency Inversion).
public interface ITaskService
{
    Task<List<TaskDto>> GetAllAsync(int? projectId, TaskItemStatus? status, CancellationToken ct = default);
    Task<TaskDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken ct = default);
    Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto, CancellationToken ct = default);
    Task<TaskDto> UpdateStatusAsync(int id, TaskItemStatus status, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
