using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Repositories;

/// <summary>
/// Board repository implementation.
/// </summary>
public class BoardRepository : Repository<Board>, IBoardRepository
{
    public BoardRepository(CloudBoardContext context) : base(context)
    {
    }

    public async Task<Board?> GetWithColumnsAsync(int id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(b => b.Columns.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<Board?> GetWithWorkItemsAsync(int id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(b => b.WorkItems)
            .Include(b => b.Columns.OrderBy(c => c.Order))
            .Include(b => b.Sprints)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<List<Board>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(b => b.ProjectId == projectId)
            .Include(b => b.WorkItems)
            .Include(b => b.Columns.OrderBy(c => c.Order))
            .OrderBy(b => b.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int?> GetProjectIdAsync(int boardId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(b => b.Id == boardId)
            .Select(b => (int?)b.ProjectId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Board?> GetWithProjectAsync(int id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(b => b.Project)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }
}
