using CloudBoard.Api.Models;

namespace CloudBoard.Api.Repositories;

/// <summary>
/// Sprint-specific data access operations.
/// </summary>
public interface ISprintRepository : IRepository<Sprint>
{
    /// <summary>
    /// Gets sprint with its work items.
    /// </summary>
    Task<Sprint?> GetWithWorkItemsAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets sprint with work items and board/project context.
    /// </summary>
    Task<Sprint?> GetWithFullContextAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets all sprints for a board.
    /// </summary>
    Task<List<Sprint>> GetByBoardAsync(int boardId, CancellationToken ct = default);

    /// <summary>
    /// Gets the active sprint for a board.
    /// </summary>
    Task<Sprint?> GetActiveSprintAsync(int boardId, CancellationToken ct = default);

    /// <summary>
    /// Checks if board has another active sprint.
    /// </summary>
    Task<bool> HasActiveSprintAsync(int boardId, int excludeSprintId, CancellationToken ct = default);
}
