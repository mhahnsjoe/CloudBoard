namespace CloudBoard.Api.Services;

using CloudBoard.Api.Common;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Models;

public interface IProjectService
{
    Task<Result<List<Project>>> GetProjectsAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<Project>> GetProjectByIdAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task<Result<Project>> CreateProjectAsync(ProjectCreateDto projectDto, int userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateProjectAsync(int id, ProjectUpdateDto projectDto, int userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteProjectAsync(int id, int userId, CancellationToken cancellationToken = default);
}
