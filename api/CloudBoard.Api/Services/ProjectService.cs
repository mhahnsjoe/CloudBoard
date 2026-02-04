using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Repositories;
using CloudBoard.Api.Common;

namespace CloudBoard.Api.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository projectRepository,
        IBoardRepository boardRepository,
        ITeamRepository teamRepository,
        ILogger<ProjectService> logger)
    {
        _projectRepository = projectRepository;
        _boardRepository = boardRepository;
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<Result<List<ProjectDto>>> GetProjectsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAccessibleByUserAsync(userId, cancellationToken);
        var dtos = projects.Select(MapToDto).ToList();
        return Result<List<ProjectDto>>.Success(dtos);
    }

    public async Task<Result<ProjectDto>> GetProjectByIdAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetWithBoardsAndWorkItemsAsync(id, cancellationToken);

        if (project == null)
            return Result<ProjectDto>.NotFound($"Project {id} not found");

        // Check team membership for access
        if (!await _teamRepository.IsMemberAsync(project.TeamId, userId, cancellationToken))
            return Result<ProjectDto>.Forbidden("You don't have access to this project");

        return Result<ProjectDto>.Success(MapToDto(project));
    }

    public async Task<Result<ProjectDto>> CreateProjectAsync(ProjectCreateDto projectDto, int userId, CancellationToken cancellationToken = default)
    {
        // Verify user is member of the specified team
        if (!await _teamRepository.IsMemberAsync(projectDto.TeamId, userId, cancellationToken))
            return Result<ProjectDto>.Forbidden("You are not a member of this team");

        var project = new Project
        {
            Name = projectDto.Name,
            Description = projectDto.Description,
            CreatedAt = DateTime.UtcNow,
            OwnerId = userId,
            TeamId = projectDto.TeamId
        };

        _projectRepository.Add(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

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

        _boardRepository.Add(defaultBoard);
        await _boardRepository.SaveChangesAsync(cancellationToken);

        // Reload with includes
        var createdProject = await _projectRepository.GetWithBoardsAsync(project.Id, cancellationToken);

        _logger.LogInformation("Created project {ProjectId} for user {UserId}", project.Id, userId);

        return Result<ProjectDto>.Success(MapToDto(createdProject!));
    }

    private static ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            TeamId = project.TeamId,
            TeamName = project.Team?.Name,
            Boards = project.Boards?.Select(b => new BoardDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Type = b.Type,
                ProjectId = b.ProjectId,
                Columns = b.Columns?.OrderBy(c => c.Order).Select(c => new BoardColumnDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Order = c.Order,
                    Category = c.Category
                }).ToList(),
                WorkItemCount = b.WorkItems?.Count ?? 0
            }).ToList()
        };
    }

    public async Task<Result> UpdateProjectAsync(int id, ProjectUpdateDto projectDto, int userId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);

        if (project == null)
            return Result.NotFound($"Project {id} not found");

        // Check team membership for access
        if (!await _teamRepository.IsMemberAsync(project.TeamId, userId, cancellationToken))
            return Result.Forbidden("You don't have access to this project");

        project.Name = projectDto.Name;
        project.Description = projectDto.Description;

        await _projectRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated project {ProjectId}", id);
        return Result.Success();
    }

    public async Task<Result> DeleteProjectAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);

        if (project == null)
            return Result.NotFound($"Project {id} not found");

        // Only team admins/owners can delete projects
        if (!await _teamRepository.HasRoleOrHigherAsync(project.TeamId, userId, TeamRole.Admin, cancellationToken))
            return Result.Forbidden("You must be a team admin to delete projects");

        _projectRepository.Remove(project);
        await _projectRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted project {ProjectId}", id);
        return Result.Success();
    }
}
