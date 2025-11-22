using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CloudBoard.Api.Services;

namespace CloudBoard.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projetService;

        public ProjectsController(IProjectService projectService)
        {
            _projetService = projectService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!);
        }

        // GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            var userId = GetCurrentUserId();
            var projects = await _projetService.GetProjectsAsync(userId);
            return projects;
        }

        // GET: api/projects/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            try
            {
                var project = await _projetService.GetProjectByIdAsync(id);
                return project;
            }
            catch(KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject(ProjectCreateDto projectDto)
        {
            var userId = GetCurrentUserId();
            try
            {
                var createdProject = await _projetService.CreateProjectAsync(projectDto, userId);
                return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id }, createdProject);
            }
            catch(Exception)
            {
                //do Something here log etc not just bad request
                //TODO: Add generic error handling
                return BadRequest();
            }
        }

        // PUT: api/projects/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectUpdateDto projectDto)
        {
            try {
                await _projetService.UpdateProjectAsync(id, projectDto);
                return NoContent();
            }
            catch(KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/projects/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                await _projetService.DeleteProjectAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}