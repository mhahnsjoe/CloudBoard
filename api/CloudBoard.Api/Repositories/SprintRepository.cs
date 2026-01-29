using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Repositories;

/// <summary>
/// Sprint repository implementation.
/// </summary>
public class SprintRepository : Repository<Sprint>, ISprintRepository
{
    public SprintRepository(CloudBoardContext context) : base(context)
    {
    }

    public async Task<Sprint?> GetWithWorkItemsAsync(int id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.WorkItems)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Sprint?> GetWithFullContextAsync(int id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(s => s.WorkItems)
            .Include(s => s.Board)
                .ThenInclude(b => b.Project)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<List<Sprint>> GetByBoardAsync(int boardId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.BoardId == boardId)
            .Include(s => s.WorkItems)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync(ct);
    }

    public async Task<Sprint?> GetActiveSprintAsync(int boardId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(s => s.BoardId == boardId && s.Status == SprintStatus.Active)
            .Include(s => s.WorkItems)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> HasActiveSprintAsync(int boardId, int excludeSprintId, CancellationToken ct = default)
    {
        return await DbSet
            .AnyAsync(s => s.BoardId == boardId && s.Status == SprintStatus.Active && s.Id != excludeSprintId, ct);
    }
}
