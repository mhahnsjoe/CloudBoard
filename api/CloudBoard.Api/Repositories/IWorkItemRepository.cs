using CloudBoard.Api.Models;

namespace CloudBoard.Api.Repositories;

/// <summary>
/// WorkItem-specific data access operations.
/// Handles hierarchical queries and backlog management.
/// </summary>
public interface IWorkItemRepository : IRepository<WorkItem>
{
    /// <summary>
    /// Gets work item with parent and children.
    /// </summary>
    Task<WorkItem?> GetWithHierarchyAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets work item with parent, children, and board/project info.
    /// </summary>
    Task<WorkItem?> GetWithFullContextAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets all work items for a board.
    /// </summary>
    Task<List<WorkItem>> GetByBoardAsync(int boardId, CancellationToken ct = default);

    /// <summary>
    /// Gets root items (no parent) for a board with children loaded.
    /// </summary>
    Task<List<WorkItem>> GetRootsByBoardAsync(int boardId, CancellationToken ct = default);

    /// <summary>
    /// Gets backlog items (no board) for a project.
    /// </summary>
    Task<List<WorkItem>> GetBacklogAsync(int projectId, CancellationToken ct = default);

    /// <summary>
    /// Gets max backlog order for calculating next order value.
    /// </summary>
    Task<int?> GetMaxBacklogOrderAsync(int projectId, int? parentId, CancellationToken ct = default);

    /// <summary>
    /// Gets work items assigned to a sprint.
    /// </summary>
    Task<List<WorkItem>> GetBySprintAsync(int sprintId, CancellationToken ct = default);

    /// <summary>
    /// Gets work items by IDs for a project backlog.
    /// </summary>
    Task<List<WorkItem>> GetBacklogItemsByIdsAsync(int projectId, List<int> itemIds, CancellationToken ct = default);
}
