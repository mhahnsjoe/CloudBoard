using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Repositories;

public class WorkItemHistoryRepository : Repository<WorkItemHistory>, IWorkItemHistoryRepository
{
    public WorkItemHistoryRepository(CloudBoardContext context) : base(context)
    {
    }

    public async Task<List<WorkItemHistory>> GetBySprintAsync(int sprintId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(h => h.WorkItem)
            .Where(h => h.WorkItem.SprintId == sprintId || (h.FieldName == "SprintId" && (h.OldValue == sprintId.ToString() || h.NewValue == sprintId.ToString())))
            .OrderBy(h => h.ChangedAt)
            .ToListAsync(ct);
    }

    public async Task<List<WorkItemHistory>> GetByWorkItemAsync(int workItemId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(h => h.WorkItemId == workItemId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync(ct);
    }
}
