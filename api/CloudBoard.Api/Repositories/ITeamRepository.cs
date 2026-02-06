using CloudBoard.Api.Models;

namespace CloudBoard.Api.Repositories;

public interface ITeamRepository : IRepository<Team>
{
    /// <summary>
    /// Gets team with all members loaded
    /// </summary>
    Task<Team?> GetWithMembersAsync(int teamId, CancellationToken ct = default);

    /// <summary>
    /// Gets team with members and projects loaded
    /// </summary>
    Task<Team?> GetWithMembersAndProjectsAsync(int teamId, CancellationToken ct = default);

    /// <summary>
    /// Gets all teams where user is a member
    /// </summary>
    Task<List<Team>> GetByUserAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Gets the user's membership record for a team
    /// </summary>
    Task<TeamMember?> GetMembershipAsync(int teamId, int userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if user is a member of the team
    /// </summary>
    Task<bool> IsMemberAsync(int teamId, int userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if user has at least the specified role
    /// </summary>
    Task<bool> HasRoleOrHigherAsync(int teamId, int userId, TeamRole minimumRole, CancellationToken ct = default);

    /// <summary>
    /// Gets pending invitations for a team
    /// </summary>
    Task<List<TeamInvitation>> GetPendingInvitationsAsync(int teamId, CancellationToken ct = default);

    /// <summary>
    /// Gets invitation by token
    /// </summary>
    Task<TeamInvitation?> GetInvitationByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Gets pending invitation for email in team
    /// </summary>
    Task<TeamInvitation?> GetPendingInvitationAsync(int teamId, string email, CancellationToken ct = default);

    /// <summary>
    /// Adds a team member
    /// </summary>
    void AddMember(TeamMember member);

    /// <summary>
    /// Removes a team member
    /// </summary>
    void RemoveMember(TeamMember member);

    /// <summary>
    /// Adds a team invitation
    /// </summary>
    void AddInvitation(TeamInvitation invitation);

    /// <summary>
    /// Removes a team invitation
    /// </summary>
    void RemoveInvitation(TeamInvitation invitation);

    /// <summary>
    /// Gets invitation by ID
    /// </summary>
    Task<TeamInvitation?> GetInvitationByIdAsync(int invitationId, CancellationToken ct = default);

    /// <summary>
    /// Gets all pending invitations for an email address
    /// </summary>
    Task<List<TeamInvitation>> GetInvitationsByEmailAsync(string email, CancellationToken ct = default);
}
