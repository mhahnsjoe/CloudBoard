namespace CloudBoard.Api.Services
{
    using CloudBoard.Api.Models.DTO;
    using CloudBoard.Api.Models;

    public interface IProjectService
    {
        Task<List<Project>> GetProjectsAsync(int userId, CancellationToken cancellationToken = default);
        Task<Project> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Project> CreateProjectAsync(ProjectCreateDto projectDto, int userId, CancellationToken cancellationToken = default);
        Task UpdateProjectAsync(int id, ProjectUpdateDto projectDto, CancellationToken cancellationToken = default);
        Task DeleteProjectAsync(int id, CancellationToken cancellationToken = default);
    }
}