using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Services;
using CloudBoard.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Asp.Versioning;

namespace CloudBoard.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!);
    }

    /// <summary>
    /// Gets all projects for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Project>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjects()
    {
        var userId = GetCurrentUserId();
        var result = await _projectService.GetProjectsAsync(userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Gets a specific project by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProject(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _projectService.GetProjectByIdAsync(id, userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Creates a new project
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Project), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject(ProjectCreateDto projectDto)
    {
        var userId = GetCurrentUserId();
        var result = await _projectService.CreateProjectAsync(projectDto, userId);
        return result.ToCreatedResult(this, nameof(GetProject), new { id = result.Value?.Id });
    }

    /// <summary>
    /// Updates an existing project
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProject(int id, ProjectUpdateDto projectDto)
    {
        var userId = GetCurrentUserId();
        var result = await _projectService.UpdateProjectAsync(id, projectDto, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.ToActionResult();
    }

    /// <summary>
    /// Deletes a project
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _projectService.DeleteProjectAsync(id, userId);

        if (result.IsSuccess)
            return NoContent();

        return result.ToActionResult();
    }
}
