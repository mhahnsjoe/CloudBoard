using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Repositories;

/// <summary>
/// Project repository implementation.
/// Inherits common CRUD from Repository, adds Project-specific queries.
/// </summary>
public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(CloudBoardContext context) : base(context)
    {
    }

    public async Task<Project?> GetWithBoardsAsync(int id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.Boards)
                .ThenInclude(b => b.Columns.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Project?> GetWithBoardsAndWorkItemsAsync(int id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.Boards)
                .ThenInclude(b => b.WorkItems)
            .Include(p => p.Boards)
                .ThenInclude(b => b.Columns.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Project>> GetByOwnerWithBoardsAsync(int userId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(p => p.OwnerId == userId)
            .Include(p => p.Boards)
                .ThenInclude(b => b.WorkItems)
            .Include(p => p.Boards)
                .ThenInclude(b => b.Columns.OrderBy(c => c.Order))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> IsOwnerAsync(int projectId, int userId, CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(p => p.Id == projectId && p.OwnerId == userId, ct);
    }
}
