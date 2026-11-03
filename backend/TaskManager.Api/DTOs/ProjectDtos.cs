using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.DTOs;

// TaskCount non esiste nell'entità Project: è calcolato nella query (vedi ProjectService).
public record ProjectDto(int Id, string Name, int TaskCount);

public record CreateProjectDto(
    [Required, StringLength(100, MinimumLength = 1)] string Name);
