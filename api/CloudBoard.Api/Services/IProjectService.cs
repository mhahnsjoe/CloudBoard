namespace CloudBoard.Api.Services;

using CloudBoard.Api.Common;
using CloudBoard.Api.Models.DTO;

public interface IProjectService
{
    Task<Result<List<ProjectDto>>> GetProjectsAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> GetProjectByIdAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> CreateProjectAsync(ProjectCreateDto projectDto, int userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateProjectAsync(int id, ProjectUpdateDto projectDto, int userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteProjectAsync(int id, int userId, CancellationToken cancellationToken = default);
}
