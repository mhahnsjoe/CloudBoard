using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Repositories;

/// <summary>
/// WorkItem repository implementation.
/// Contains complex hierarchical and backlog queries.
/// </summary>
public class WorkItemRepository : Repository<WorkItem>, IWorkItemRepository
{
    public WorkItemRepository(CloudBoardContext context) : base(context)
    {
    }

    public async Task<WorkItem?> GetWithHierarchyAsync(int id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(w => w.Parent)
            .Include(w => w.Children)
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<WorkItem?> GetWithFullContextAsync(int id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(w => w.Parent)
            .Include(w => w.Children)
            .Include(w => w.Board)
                .ThenInclude(b => b!.Project)
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<List<WorkItem>> GetByBoardAsync(int boardId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.BoardId == boardId)
            .Include(w => w.Children)
                .ThenInclude(c => c.Children)
                    .ThenInclude(c => c.Children)
            .OrderBy(w => w.Status)
            .ThenBy(w => w.Priority)
            .ToListAsync(ct);
    }

    public async Task<List<WorkItem>> GetRootsByBoardAsync(int boardId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.BoardId == boardId && w.ParentId == null)
            .Include(w => w.Children)
                .ThenInclude(c => c.Children)
                    .ThenInclude(c => c.Children)
            .OrderBy(w => w.Type)
            .ThenBy(w => w.Id)
            .ToListAsync(ct);
    }

    public async Task<List<WorkItem>> GetBacklogAsync(int projectId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.ProjectId == projectId && w.BoardId == null)
            .Include(w => w.Children)
            .OrderBy(w => w.BacklogOrder ?? int.MaxValue)
            .ThenBy(w => w.Id)
            .ToListAsync(ct);
    }

    public async Task<int?> GetMaxBacklogOrderAsync(int projectId, int? parentId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.ProjectId == projectId && w.BoardId == null && w.ParentId == parentId)
            .MaxAsync(w => (int?)w.BacklogOrder, ct);
    }

    public async Task<List<WorkItem>> GetBySprintAsync(int sprintId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => w.SprintId == sprintId)
            .Include(w => w.Children)
            .OrderBy(w => w.Status)
            .ToListAsync(ct);
    }

    public async Task<List<WorkItem>> GetBacklogItemsByIdsAsync(int projectId, List<int> itemIds, CancellationToken ct = default)
    {
        return await DbSet
            .Where(w => itemIds.Contains(w.Id) && w.ProjectId == projectId && w.BoardId == null)
            .ToListAsync(ct);
    }
}
