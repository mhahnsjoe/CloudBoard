using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Data;
using CloudBoard.Api.Common;

namespace CloudBoard.Api.Services;

public class ProjectService : IProjectService
{
    private readonly CloudBoardContext _context;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(CloudBoardContext context, ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Project>>> GetProjectsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var projects = await _context.Projects.GetByOwnerWithBoardsAsync(userId, cancellationToken);
        return Result<List<Project>>.Success(projects);
    }

    public async Task<Result<Project>> GetProjectByIdAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.GetProjectWithBoardsAsync(id, cancellationToken);

        if (project == null)
            return Result<Project>.NotFound($"Project {id} not found");

        if (project.OwnerId != userId)
            return Result<Project>.Forbidden("You don't have access to this project");

        return Result<Project>.Success(project);
    }

    public async Task<Result<Project>> CreateProjectAsync(ProjectCreateDto projectDto, int userId, CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            Name = projectDto.Name,
            Description = projectDto.Description,
            CreatedAt = DateTime.UtcNow,
            OwnerId = userId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        // Create default Kanban board
        var defaultBoard = new Board
        {
            Name = projectDto.Name,
            Description = "Default Kanban board",
            Type = BoardType.Kanban,
            ProjectId = project.Id,
            CreatedAt = DateTime.UtcNow
        };

        // Add default columns
        defaultBoard.Columns = new List<BoardColumn>
        {
            new() { Name = "To Do", Category = "To Do", Order = 0 },
            new() { Name = "In Progress", Category = "In Progress", Order = 1 },
            new() { Name = "Done", Category = "Done", Order = 2 }
        };

        _context.Boards.Add(defaultBoard);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with includes
        var createdProject = await _context.Projects
            .Include(p => p.Boards)
            .FirstAsync(p => p.Id == project.Id, cancellationToken);

        _logger.LogInformation("Created project {ProjectId} for user {UserId}", project.Id, userId);

        return Result<Project>.Success(createdProject);
    }

    public async Task<Result> UpdateProjectAsync(int id, ProjectUpdateDto projectDto, int userId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FindAsync(new object[] { id }, cancellationToken);

        if (project == null)
            return Result.NotFound($"Project {id} not found");

        if (project.OwnerId != userId)
            return Result.Forbidden("You don't have access to this project");

        project.Name = projectDto.Name;
        project.Description = projectDto.Description;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated project {ProjectId}", id);
        return Result.Success();
    }

    public async Task<Result> DeleteProjectAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FindAsync(new object[] { id }, cancellationToken);

        if (project == null)
            return Result.NotFound($"Project {id} not found");

        if (project.OwnerId != userId)
            return Result.Forbidden("You don't have access to this project");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted project {ProjectId}", id);
        return Result.Success();
    }
}
