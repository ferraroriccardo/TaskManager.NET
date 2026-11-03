using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Models;

namespace TaskManager.Api.DTOs;

// DTO = Data Transfer Object: la "forma" dei dati che viaggiano sulla rete.
// Non esponiamo mai le entità direttamente, perché:
//  1. sicurezza: un'entità può contenere campi che il client non deve vedere/modificare
//  2. niente cicli JSON (TaskItem -> Project -> Tasks -> TaskItem -> ...)
//  3. disaccoppiamento: cambiare il DB non rompe il contratto con il frontend
//
// "record" = classe immutabile con uguaglianza per valore, perfetta per i DTO.

// OUTPUT: ciò che l'API restituisce a React
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

// INPUT: ciò che React invia per creare un task.
// Gli attributi [Required], [StringLength]... vengono controllati automaticamente
// grazie a [ApiController]: se non sono rispettati, l'API risponde 400 Bad Request
// senza nemmeno entrare nel metodo del controller.
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

// Usato per spostare un task tra le colonne della board (PATCH: modifica parziale).
public record UpdateTaskStatusDto(TaskItemStatus Status);
