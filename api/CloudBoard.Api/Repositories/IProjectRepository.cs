using CloudBoard.Api.Models;

namespace CloudBoard.Api.Repositories;

/// <summary>
/// Project-specific data access operations.
/// Extends base repository with domain-specific queries.
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>
    /// Gets project with boards and their columns.
    /// </summary>
    Task<Project?> GetWithBoardsAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets project with boards, columns, and work items.
    /// </summary>
    Task<Project?> GetWithBoardsAndWorkItemsAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets all projects for a user with board details.
    /// </summary>
    Task<List<Project>> GetByOwnerWithBoardsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if user owns the project.
    /// </summary>
    Task<bool> IsOwnerAsync(int projectId, int userId, CancellationToken ct = default);
}
