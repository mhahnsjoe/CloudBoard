using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Data;

/// <summary>
/// Reusable query extensions for common data access patterns.
/// Keeps queries DRY without full repository abstraction.
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Includes boards with their columns (ordered) and work items.
    /// </summary>
    public static IQueryable<Project> IncludeBoardsWithDetails(this IQueryable<Project> query)
    {
        return query
            .Include(p => p.Boards)
                .ThenInclude(b => b.Columns.OrderBy(c => c.Order))
            .Include(p => p.Boards)
                .ThenInclude(b => b.WorkItems);
    }

    /// <summary>
    /// Includes columns ordered by their Order property.
    /// </summary>
    public static IQueryable<Board> IncludeOrderedColumns(this IQueryable<Board> query)
    {
        return query.Include(b => b.Columns.OrderBy(c => c.Order));
    }

    /// <summary>
    /// Includes work items with their hierarchical children.
    /// </summary>
    public static IQueryable<Board> IncludeWorkItemsWithChildren(this IQueryable<Board> query)
    {
        return query
            .Include(b => b.WorkItems)
                .ThenInclude(w => w.Children);
    }

    /// <summary>
    /// Gets a project by ID with full board details.
    /// </summary>
    public static async Task<Project?> GetProjectWithBoardsAsync(
        this DbSet<Project> projects,
        int id,
        CancellationToken ct = default)
    {
        return await projects
            .IncludeBoardsWithDetails()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    /// <summary>
    /// Gets all projects for a user with board details.
    /// </summary>
    public static async Task<List<Project>> GetByOwnerWithBoardsAsync(
        this DbSet<Project> projects,
        int userId,
        CancellationToken ct = default)
    {
        return await projects
            .Where(p => p.OwnerId == userId)
            .IncludeBoardsWithDetails()
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets backlog items for a project ordered by BacklogOrder.
    /// </summary>
    public static async Task<List<WorkItem>> GetBacklogAsync(
        this DbSet<WorkItem> workItems,
        int projectId,
        CancellationToken ct = default)
    {
        return await workItems
            .Where(w => w.ProjectId == projectId && w.BoardId == null)
            .Include(w => w.Children)
            .OrderBy(w => w.BacklogOrder ?? int.MaxValue)
            .ThenBy(w => w.Id)
            .ToListAsync(ct);
    }
}
