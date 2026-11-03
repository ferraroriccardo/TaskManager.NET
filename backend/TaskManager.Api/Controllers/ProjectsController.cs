using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.DTOs;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    // GET /api/projects
    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _projectService.GetAllAsync(ct));
    }

    // POST /api/projects   body: { "name": "..." }
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectDto dto, CancellationToken ct)
    {
        var created = await _projectService.CreateAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    // DELETE /api/projects/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _projectService.DeleteAsync(id, ct);
        return NoContent();
    }
}
