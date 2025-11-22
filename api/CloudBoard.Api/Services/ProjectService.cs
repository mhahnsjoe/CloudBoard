using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Data;

namespace CloudBoard.Api.Services
{

    public class ProjectService : IProjectService
    {
        private readonly CloudBoardContext _context;
        
        //TODO: Feature/Add-Logging-To-Backend
        //private readonly ILogger<ProjectService> _logger;

        public ProjectService(CloudBoardContext context, ILogger<ProjectService> logger)
        {
            _context = context;
        }

        public async Task<List<Project>> GetProjectsAsync(int userId, CancellationToken cancellationToken = default)
        {
            var projects = await _context.Projects.Where(p => p.OwnerId == userId)
                .Include(p => p.Boards)
                    .ThenInclude(b => b.WorkItems)
                .ToListAsync();
            
            return projects;
        }

        public async Task<Project> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var project = await _context.Projects
                .Include(p => p.Boards)
                    .ThenInclude(b => b.WorkItems)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (project == null) 
                throw new NullReferenceException();
            
            return project;
        }

        public async Task<Project> CreateProjectAsync(ProjectCreateDto projectDto, int userId, CancellationToken cancellationToken = default)
        {
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
            
            return createdProject;
        }

        public async Task UpdateProjectAsync(int id, ProjectUpdateDto projectDto, CancellationToken cancellationToken = default)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                throw new NullReferenceException();
            
            project.Name = projectDto.Name;
            project.Description = projectDto.Description;

            _context.Entry(project).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(int id, CancellationToken cancellationToken = default)
        {
            var project = await _context.Projects.FindAsync(new object[] { id }, cancellationToken).ConfigureAwait(false);
            if (project == null)
                throw new NullReferenceException();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}