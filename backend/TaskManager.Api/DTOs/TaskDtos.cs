using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Models;

namespace TaskManager.Api.DTOs;

// DTO = Data Transfer Object: the "shape" of the data travelling over the network.
// We never expose entities directly, because:
//  1. security: an entity may contain fields the client must not see/modify
//  2. no JSON cycles (TaskItem -> Project -> Tasks -> TaskItem -> ...)
//  3. decoupling: changing the DB doesn't break the contract with the frontend
//
// "record" = immutable class with value equality, perfect for DTOs.

// OUTPUT: what the API returns to React
public record TaskDto(
    int Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    DateTime CreatedAt,
    int ProjectId,
    string ProjectName);

// INPUT: what React sends to create a task.
// The [Required], [StringLength]... attributes are checked automatically
// thanks to [ApiController]: if they aren't satisfied, the API responds with 400 Bad Request
// without even entering the controller method.
public record CreateTaskDto(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    [StringLength(2000)] string? Description,
    TaskPriority Priority,
    DateTime? DueDate,
    [Range(1, int.MaxValue)] int ProjectId);

public record UpdateTaskDto(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    [StringLength(2000)] string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate);

// Used to move a task between the board columns (PATCH: partial update).
public record UpdateTaskStatusDto(TaskItemStatus Status);
