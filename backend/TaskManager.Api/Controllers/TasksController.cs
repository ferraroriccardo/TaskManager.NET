using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

// CONTROLLER: il livello HTTP. Il suo unico compito è:
//  1. ricevere la richiesta (routing + model binding JSON -> DTO)
//  2. delegare al service
//  3. tradurre il risultato in uno status code HTTP
// Niente business logic e niente accesso diretto al DbContext qui ("thin controller").
//
// [ApiController] attiva: validazione automatica dei DTO (400 se non validi),
// binding automatico del body JSON, risposte di errore in formato ProblemDetails.
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // GET /api/tasks?projectId=1&status=Todo
    [HttpGet]
    public async Task<ActionResult<List<TaskDto>>> GetAll(
        [FromQuery] int? projectId, [FromQuery] TaskItemStatus? status, CancellationToken ct)
    {
        // CancellationToken: se il client chiude la connessione, la query al DB viene annullata.
        return Ok(await _taskService.GetAllAsync(projectId, status, ct));
    }

    // GET /api/tasks/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetById(int id, CancellationToken ct)
    {
        // Se il task non esiste, il service lancia NotFoundException -> GlobalExceptionHandler -> 404
        return Ok(await _taskService.GetByIdAsync(id, ct));
    }

    // POST /api/tasks   body: { "title": "...", "projectId": 1, ... }
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto, CancellationToken ct)
    {
        var created = await _taskService.CreateAsync(dto, ct);
        // 201 Created + header "Location: /api/tasks/{id}" (convenzione REST)
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/tasks/5  -> sostituisce tutti i campi modificabili
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskDto>> Update(int id, UpdateTaskDto dto, CancellationToken ct)
    {
        return Ok(await _taskService.UpdateAsync(id, dto, ct));
    }

    // PATCH /api/tasks/5/status  -> modifica parziale: solo lo stato (spostamento sulla board)
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskDto>> UpdateStatus(int id, UpdateTaskStatusDto dto, CancellationToken ct)
    {
        return Ok(await _taskService.UpdateStatusAsync(id, dto.Status, ct));
    }

    // DELETE /api/tasks/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _taskService.DeleteAsync(id, ct);
        return NoContent(); // 204: fatto, niente da restituire
    }
}
