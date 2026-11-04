using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs;

// TaskCount doesn't exist on the Project entity: it's computed in the query (see ProjectService).
public record ProjectDto(int Id, string Name, int TaskCount);

public record CreateProjectDto(
    [Required, StringLength(100, MinimumLength = 1)] string Name);
