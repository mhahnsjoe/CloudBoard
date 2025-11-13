using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CloudBoard.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly CloudBoardContext _context;

        public ProjectsController(CloudBoardContext context)
        {
            _context = context;
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
            return await _context.Projects.Where(p => p.OwnerId == userId)
                .Include(p => p.Boards)
                    .ThenInclude(b => b.WorkItems)
                .ToListAsync();
        }

        // GET: api/projects/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Boards)
                    .ThenInclude(b => b.WorkItems)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (project == null) 
                return NotFound();
            
            return project;
        }

        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject(ProjectCreateDto projectDto)
        {
            var userId = GetCurrentUserId();
            var project = new Project
            {
                Name = projectDto.Name,
                Description = projectDto.Description,
                CreatedAt = DateTime.UtcNow,
                OwnerId = userId
            };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Automatically create a default Kanban board for the project
            var defaultBoard = new Board
            {
                Name = "Main Board",
                Description = "Default Kanban board",
                Type = BoardType.Kanban,
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Boards.Add(defaultBoard);
            await _context.SaveChangesAsync();

            // Reload project with board
            var createdProject = await _context.Projects
                .Include(p => p.Boards)
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, createdProject);
        }

        // PUT: api/projects/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectUpdateDto projectDto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            project.Name = projectDto.Name;
            project.Description = projectDto.Description;

            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        // DELETE: api/projects/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) 
                return NotFound();
            
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}