using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

// CONTROLLER: the HTTP layer. Its only job is to:
//  1. receive the request (routing + JSON -> DTO model binding)
//  2. delegate to the service
//  3. translate the result into an HTTP status code
// No business logic and no direct DbContext access here ("thin controller").
//
// [ApiController] enables: automatic DTO validation (400 if invalid),
// automatic JSON body binding, error responses in ProblemDetails format.
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
        // CancellationToken: if the client closes the connection, the DB query is cancelled.
        return Ok(await _taskService.GetAllAsync(projectId, status, ct));
    }

    // GET /api/tasks/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetById(int id, CancellationToken ct)
    {
        // If the task doesn't exist, the service throws NotFoundException -> GlobalExceptionHandler -> 404
        return Ok(await _taskService.GetByIdAsync(id, ct));
    }

    // POST /api/tasks   body: { "title": "...", "projectId": 1, ... }
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto, CancellationToken ct)
    {
        var created = await _taskService.CreateAsync(dto, ct);
        // 201 Created + header "Location: /api/tasks/{id}" (REST convention)
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/tasks/5  -> replaces all editable fields
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskDto>> Update(int id, UpdateTaskDto dto, CancellationToken ct)
    {
        return Ok(await _taskService.UpdateAsync(id, dto, ct));
    }

    // PATCH /api/tasks/5/status  -> partial update: status only (moving on the board)
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
        return NoContent(); // 204: done, nothing to return
    }
}
