using CloudBoard.Api.Common;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;

namespace CloudBoard.Api.Services;

public interface ITeamService
{
    // Team CRUD
    Task<Result<List<TeamDto>>> GetUserTeamsAsync(int userId, CancellationToken ct = default);
    Task<Result<TeamDetailDto>> GetTeamByIdAsync(int teamId, int userId, CancellationToken ct = default);
    Task<Result<TeamDto>> CreateTeamAsync(CreateTeamDto dto, int userId, CancellationToken ct = default);
    Task<Result> UpdateTeamAsync(int teamId, UpdateTeamDto dto, int userId, CancellationToken ct = default);
    Task<Result> DeleteTeamAsync(int teamId, int userId, CancellationToken ct = default);

    // Member management
    Task<Result<TeamInvitationDto>> InviteMemberAsync(int teamId, InviteMemberDto dto, int userId, CancellationToken ct = default);
    Task<Result> CancelInvitationAsync(int teamId, int invitationId, int userId, CancellationToken ct = default);
    Task<Result<TeamMemberDto>> AcceptInvitationAsync(string token, int userId, CancellationToken ct = default);
    Task<Result> UpdateMemberRoleAsync(int teamId, int targetUserId, UpdateMemberRoleDto dto, int userId, CancellationToken ct = default);
    Task<Result> RemoveMemberAsync(int teamId, int targetUserId, int userId, CancellationToken ct = default);
    Task<Result> LeaveTeamAsync(int teamId, int userId, CancellationToken ct = default);

    // User's invitations
    Task<Result<List<MyInvitationDto>>> GetMyInvitationsAsync(string email, CancellationToken ct = default);
    Task<Result> DeclineInvitationAsync(int invitationId, int userId, CancellationToken ct = default);

    // Authorization helpers
    Task<bool> CanAccessTeamAsync(int teamId, int userId, CancellationToken ct = default);
    Task<bool> CanManageMembersAsync(int teamId, int userId, CancellationToken ct = default);
    Task<bool> CanDeleteTeamAsync(int teamId, int userId, CancellationToken ct = default);
}
