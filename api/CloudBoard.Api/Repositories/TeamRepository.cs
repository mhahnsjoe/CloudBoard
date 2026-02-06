using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Repositories;

public class TeamRepository : Repository<Team>, ITeamRepository
{
    public TeamRepository(CloudBoardContext context) : base(context)
    {
    }

    public async Task<Team?> GetWithMembersAsync(int teamId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == teamId, ct);
    }

    public async Task<Team?> GetWithMembersAndProjectsAsync(int teamId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Projects)
            .Include(t => t.CreatedBy)
            .Include(t => t.Invitations.Where(i => i.AcceptedAt == null && i.ExpiresAt > DateTime.UtcNow))
                .ThenInclude(i => i.InvitedBy)
            .FirstOrDefaultAsync(t => t.Id == teamId, ct);
    }

    public async Task<List<Team>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        return await DbSet
            .Where(t => t.Members.Any(m => m.UserId == userId))
            .Include(t => t.Members)
            .Include(t => t.Projects)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<TeamMember?> GetMembershipAsync(int teamId, int userId, CancellationToken ct = default)
    {
        return await Context.TeamMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId, ct);
    }

    public async Task<bool> IsMemberAsync(int teamId, int userId, CancellationToken ct = default)
    {
        return await Context.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.UserId == userId, ct);
    }

    public async Task<bool> HasRoleOrHigherAsync(int teamId, int userId, TeamRole minimumRole, CancellationToken ct = default)
    {
        return await Context.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.UserId == userId && m.Role >= minimumRole, ct);
    }

    public async Task<List<TeamInvitation>> GetPendingInvitationsAsync(int teamId, CancellationToken ct = default)
    {
        return await Context.TeamInvitations
            .Where(i => i.TeamId == teamId && i.AcceptedAt == null && i.ExpiresAt > DateTime.UtcNow)
            .Include(i => i.InvitedBy)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<TeamInvitation?> GetInvitationByTokenAsync(string token, CancellationToken ct = default)
    {
        return await Context.TeamInvitations
            .Include(i => i.Team)
            .Include(i => i.InvitedBy)
            .FirstOrDefaultAsync(i => i.Token == token, ct);
    }

    public async Task<TeamInvitation?> GetPendingInvitationAsync(int teamId, string email, CancellationToken ct = default)
    {
        return await Context.TeamInvitations
            .FirstOrDefaultAsync(i =>
                i.TeamId == teamId &&
                i.Email.ToLower() == email.ToLower() &&
                i.AcceptedAt == null &&
                i.ExpiresAt > DateTime.UtcNow, ct);
    }

    public void AddMember(TeamMember member)
    {
        Context.TeamMembers.Add(member);
    }

    public void RemoveMember(TeamMember member)
    {
        Context.TeamMembers.Remove(member);
    }

    public void AddInvitation(TeamInvitation invitation)
    {
        Context.TeamInvitations.Add(invitation);
    }

    public void RemoveInvitation(TeamInvitation invitation)
    {
        Context.TeamInvitations.Remove(invitation);
    }

    public async Task<TeamInvitation?> GetInvitationByIdAsync(int invitationId, CancellationToken ct = default)
    {
        return await Context.TeamInvitations
            .Include(i => i.Team)
            .FirstOrDefaultAsync(i => i.Id == invitationId, ct);
    }

    public async Task<List<TeamInvitation>> GetInvitationsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await Context.TeamInvitations
            .Where(i => i.Email.ToLower() == email.ToLower() && i.AcceptedAt == null && i.ExpiresAt > DateTime.UtcNow)
            .Include(i => i.Team)
            .Include(i => i.InvitedBy)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }
}
