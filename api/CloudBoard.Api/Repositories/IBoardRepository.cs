using CloudBoard.Api.Models;

namespace CloudBoard.Api.Repositories;

/// <summary>
/// Board-specific data access operations.
/// </summary>
public interface IBoardRepository : IRepository<Board>
{
    /// <summary>
    /// Gets board with ordered columns.
    /// </summary>
    Task<Board?> GetWithColumnsAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets board with work items and columns.
    /// </summary>
    Task<Board?> GetWithWorkItemsAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets all boards for a project with details.
    /// </summary>
    Task<List<Board>> GetByProjectAsync(int projectId, CancellationToken ct = default);

    /// <summary>
    /// Gets project ID for authorization checks.
    /// </summary>
    Task<int?> GetProjectIdAsync(int boardId, CancellationToken ct = default);

    /// <summary>
    /// Gets board with its parent project for authorization checks.
    /// </summary>
    Task<Board?> GetWithProjectAsync(int id, CancellationToken ct = default);
}
