using CloudBoard.Api.Common;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Repositories;
using System.Security.Cryptography;

namespace CloudBoard.Api.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepository;
    private readonly ILogger<TeamService> _logger;
    private const int InvitationExpirationDays = 7;

    public TeamService(ITeamRepository teamRepository, ILogger<TeamService> logger)
    {
        _teamRepository = teamRepository;
        _logger = logger;
    }

    public async Task<Result<List<TeamDto>>> GetUserTeamsAsync(int userId, CancellationToken ct = default)
    {
        _logger.LogDebug("Getting teams for user {UserId}", userId);

        var teams = await _teamRepository.GetByUserAsync(userId, ct);

        var dtos = teams.Select(t => new TeamDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            CreatedAt = t.CreatedAt,
            MemberCount = t.Members.Count,
            ProjectCount = t.Projects.Count,
            CurrentUserRole = t.Members.First(m => m.UserId == userId).Role
        }).ToList();

        return Result<List<TeamDto>>.Success(dtos);
    }

    public async Task<Result<TeamDetailDto>> GetTeamByIdAsync(int teamId, int userId, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetWithMembersAndProjectsAsync(teamId, ct);

        if (team == null)
            return Result<TeamDetailDto>.NotFound("Team not found");

        var membership = team.Members.FirstOrDefault(m => m.UserId == userId);
        if (membership == null)
            return Result<TeamDetailDto>.Forbidden("You are not a member of this team");

        var dto = new TeamDetailDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            CreatedAt = team.CreatedAt,
            CreatedBy = new UserSummaryDto
            {
                Id = team.CreatedBy.Id,
                Name = team.CreatedBy.Name,
                Email = team.CreatedBy.Email!
            },
            Members = team.Members.Select(m => new TeamMemberDto
            {
                UserId = m.UserId,
                Name = m.User.Name,
                Email = m.User.Email!,
                Role = m.Role,
                JoinedAt = m.JoinedAt
            }).OrderByDescending(m => m.Role).ThenBy(m => m.Name).ToList(),
            Projects = team.Projects.Select(p => new ProjectSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                CreatedAt = p.CreatedAt
            }).ToList(),
            PendingInvitations = membership.Role >= TeamRole.Admin
                ? team.Invitations.Select(i => new TeamInvitationDto
                {
                    Id = i.Id,
                    Email = i.Email,
                    Role = i.Role,
                    CreatedAt = i.CreatedAt,
                    ExpiresAt = i.ExpiresAt,
                    InvitedBy = new UserSummaryDto
                    {
                        Id = i.InvitedBy.Id,
                        Name = i.InvitedBy.Name,
                        Email = i.InvitedBy.Email!
                    },
                    IsExpired = i.IsExpired
                }).ToList()
                : new List<TeamInvitationDto>(),
            CurrentUserRole = membership.Role
        };

        return Result<TeamDetailDto>.Success(dto);
    }

    public async Task<Result<TeamDto>> CreateTeamAsync(CreateTeamDto dto, int userId, CancellationToken ct = default)
    {
        _logger.LogInformation("User {UserId} creating team {TeamName}", userId, dto.Name);

        var team = new Team
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId
        };

        _teamRepository.Add(team);

        // Add creator as Owner
        var membership = new TeamMember
        {
            Team = team,
            UserId = userId,
            Role = TeamRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        _teamRepository.AddMember(membership);
        await _teamRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Team {TeamId} created successfully", team.Id);

        return Result<TeamDto>.Success(new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            CreatedAt = team.CreatedAt,
            MemberCount = 1,
            ProjectCount = 0,
            CurrentUserRole = TeamRole.Owner
        });
    }

    public async Task<Result> UpdateTeamAsync(int teamId, UpdateTeamDto dto, int userId, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetByIdAsync(teamId, ct);

        if (team == null)
            return Result.NotFound("Team not found");

        if (!await _teamRepository.HasRoleOrHigherAsync(teamId, userId, TeamRole.Admin, ct))
            return Result.Forbidden("You must be an admin to update team settings");

        if (dto.Name != null)
            team.Name = dto.Name;
        if (dto.Description != null)
            team.Description = dto.Description;

        await _teamRepository.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} updated team {TeamId}", userId, teamId);

        return Result.Success();
    }

    public async Task<Result> DeleteTeamAsync(int teamId, int userId, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetWithMembersAndProjectsAsync(teamId, ct);

        if (team == null)
            return Result.NotFound("Team not found");

        if (!await _teamRepository.HasRoleOrHigherAsync(teamId, userId, TeamRole.Owner, ct))
            return Result.Forbidden("Only the team owner can delete the team");

        if (team.Projects.Any())
            return Result.BadRequest("Cannot delete team with existing projects. Delete or transfer projects first.");

        _logger.LogInformation("User {UserId} deleting team {TeamId}", userId, teamId);

        _teamRepository.Remove(team);
        await _teamRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<TeamInvitationDto>> InviteMemberAsync(int teamId, InviteMemberDto dto, int userId, CancellationToken ct = default)
    {
        var team = await _teamRepository.GetWithMembersAsync(teamId, ct);

        if (team == null)
            return Result<TeamInvitationDto>.NotFound("Team not found");

        var inviterMembership = team.Members.FirstOrDefault(m => m.UserId == userId);
        if (inviterMembership == null || inviterMembership.Role < TeamRole.Admin)
            return Result<TeamInvitationDto>.Forbidden("You must be an admin to invite members");

        // Can't invite with higher role than yourself (except Owner can do anything)
        if (dto.Role > inviterMembership.Role && inviterMembership.Role != TeamRole.Owner)
            return Result<TeamInvitationDto>.Forbidden("Cannot invite member with higher role than yourself");

        // Check if already a member
        if (team.Members.Any(m => m.User.Email!.Equals(dto.Email, StringComparison.OrdinalIgnoreCase)))
            return Result<TeamInvitationDto>.BadRequest("User is already a member of this team");

        // Check for existing pending invitation
        var existingInvitation = await _teamRepository.GetPendingInvitationAsync(teamId, dto.Email, ct);
        if (existingInvitation != null)
            return Result<TeamInvitationDto>.BadRequest("An invitation is already pending for this email");

        var invitation = new TeamInvitation
        {
            TeamId = teamId,
            Email = dto.Email,
            Role = dto.Role,
            Token = GenerateInvitationToken(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(InvitationExpirationDays),
            InvitedById = userId
        };

        _teamRepository.AddInvitation(invitation);
        await _teamRepository.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} invited {Email} to team {TeamId}", userId, dto.Email, teamId);

        return Result<TeamInvitationDto>.Success(new TeamInvitationDto
        {
            Id = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            InvitedBy = new UserSummaryDto
            {
                Id = userId,
                Name = inviterMembership.User.Name,
                Email = inviterMembership.User.Email!
            },
            IsExpired = false,
            Token = invitation.Token // Return token so it can be shared
        });
    }

    public async Task<Result<TeamMemberDto>> AcceptInvitationAsync(string token, int userId, CancellationToken ct = default)
    {
        var invitation = await _teamRepository.GetInvitationByTokenAsync(token, ct);

        if (invitation == null)
            return Result<TeamMemberDto>.NotFound("Invalid invitation");

        if (invitation.IsExpired)
            return Result<TeamMemberDto>.BadRequest("This invitation has expired");

        if (invitation.AcceptedAt != null)
            return Result<TeamMemberDto>.BadRequest("This invitation has already been used");

        // Check if already a member
        if (await _teamRepository.IsMemberAsync(invitation.TeamId, userId, ct))
            return Result<TeamMemberDto>.BadRequest("You are already a member of this team");

        var membership = new TeamMember
        {
            TeamId = invitation.TeamId,
            UserId = userId,
            Role = invitation.Role,
            JoinedAt = DateTime.UtcNow,
            InvitedById = invitation.InvitedById
        };

        invitation.AcceptedAt = DateTime.UtcNow;
        _teamRepository.AddMember(membership);

        await _teamRepository.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} accepted invitation to team {TeamId}", userId, invitation.TeamId);

        // Fetch team with members to get user info
        var team = await _teamRepository.GetWithMembersAsync(invitation.TeamId, ct);
        var member = team!.Members.First(m => m.UserId == userId);

        return Result<TeamMemberDto>.Success(new TeamMemberDto
        {
            UserId = member.UserId,
            Name = member.User.Name,
            Email = member.User.Email!,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        });
    }

    public async Task<Result> CancelInvitationAsync(int teamId, int invitationId, int userId, CancellationToken ct = default)
    {
        if (!await _teamRepository.HasRoleOrHigherAsync(teamId, userId, TeamRole.Admin, ct))
            return Result.Forbidden("You must be an admin to cancel invitations");

        var invitation = await _teamRepository.GetInvitationByIdAsync(invitationId, ct);

        if (invitation == null || invitation.TeamId != teamId)
            return Result.NotFound("Invitation not found");

        if (invitation.AcceptedAt != null)
            return Result.BadRequest("This invitation has already been accepted");

        _teamRepository.RemoveInvitation(invitation);
        await _teamRepository.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} cancelled invitation {InvitationId} for team {TeamId}", userId, invitationId, teamId);

        return Result.Success();
    }

    public async Task<Result> UpdateMemberRoleAsync(int teamId, int targetUserId, UpdateMemberRoleDto dto, int userId, CancellationToken ct = default)
    {
        if (userId == targetUserId)
            return Result.BadRequest("Cannot change your own role");

        var team = await _teamRepository.GetWithMembersAsync(teamId, ct);

        if (team == null)
            return Result.NotFound("Team not found");

        var actorMembership = team.Members.FirstOrDefault(m => m.UserId == userId);
        var targetMembership = team.Members.FirstOrDefault(m => m.UserId == targetUserId);

        if (actorMembership == null)
            return Result.Forbidden("You are not a member of this team");

        if (targetMembership == null)
            return Result.NotFound("Target user is not a member of this team");

        // Only Admin or higher can change roles
        if (actorMembership.Role < TeamRole.Admin)
            return Result.Forbidden("You must be an admin to change member roles");

        if (targetMembership.Role == TeamRole.Owner)
            return Result.BadRequest("Cannot change the owner's role");

        if (dto.Role == TeamRole.Owner)
            return Result.BadRequest("Use transfer ownership instead");

        if (dto.Role >= actorMembership.Role && actorMembership.Role != TeamRole.Owner)
            return Result.Forbidden("Cannot promote member to role equal or higher than yours");

        targetMembership.Role = dto.Role;
        await _teamRepository.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} changed role of user {TargetId} to {Role} in team {TeamId}",
            userId, targetUserId, dto.Role, teamId);

        return Result.Success();
    }

    public async Task<Result> RemoveMemberAsync(int teamId, int targetUserId, int userId, CancellationToken ct = default)
    {
        if (userId == targetUserId)
            return await LeaveTeamAsync(teamId, userId, ct);

        var team = await _teamRepository.GetWithMembersAsync(teamId, ct);

        if (team == null)
            return Result.NotFound("Team not found");

        var actorMembership = team.Members.FirstOrDefault(m => m.UserId == userId);
        var targetMembership = team.Members.FirstOrDefault(m => m.UserId == targetUserId);

        if (actorMembership == null || actorMembership.Role < TeamRole.Admin)
            return Result.Forbidden("You must be an admin to remove members");

        if (targetMembership == null)
            return Result.NotFound("User is not a member of this team");

        if (targetMembership.Role == TeamRole.Owner)
            return Result.BadRequest("Cannot remove the team owner");

        if (targetMembership.Role >= actorMembership.Role && actorMembership.Role != TeamRole.Owner)
            return Result.Forbidden("Cannot remove member with equal or higher role");

        _teamRepository.RemoveMember(targetMembership);
        await _teamRepository.SaveChangesAsync(ct);

        _logger.LogInformation("User {ActorId} removed user {TargetId} from team {TeamId}", userId, targetUserId, teamId);

        return Result.Success();
    }

    public async Task<Result> LeaveTeamAsync(int teamId, int userId, CancellationToken ct = default)
    {
        var membership = await _teamRepository.GetMembershipAsync(teamId, userId, ct);

        if (membership == null)
            return Result.NotFound("You are not a member of this team");

        if (membership.Role == TeamRole.Owner)
            return Result.BadRequest("Team owner cannot leave. Transfer ownership first or delete the team.");

        _teamRepository.RemoveMember(membership);
        await _teamRepository.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} left team {TeamId}", userId, teamId);

        return Result.Success();
    }

    public async Task<Result<List<MyInvitationDto>>> GetMyInvitationsAsync(string email, CancellationToken ct = default)
    {
        var invitations = await _teamRepository.GetInvitationsByEmailAsync(email, ct);

        var dtos = invitations.Select(i => new MyInvitationDto
        {
            Id = i.Id,
            TeamId = i.TeamId,
            TeamName = i.Team.Name,
            TeamDescription = i.Team.Description,
            Role = i.Role,
            CreatedAt = i.CreatedAt,
            ExpiresAt = i.ExpiresAt,
            InvitedBy = new UserSummaryDto
            {
                Id = i.InvitedBy.Id,
                Name = i.InvitedBy.Name,
                Email = i.InvitedBy.Email
            },
            Token = i.Token
        }).ToList();

        return Result<List<MyInvitationDto>>.Success(dtos);
    }

    public async Task<Result> DeclineInvitationAsync(int invitationId, int userId, CancellationToken ct = default)
    {
        var invitation = await _teamRepository.GetInvitationByIdAsync(invitationId, ct);

        if (invitation == null)
            return Result.NotFound("Invitation not found");

        // User can only decline invitations sent to their email - we need to verify this
        // by checking if the invitation email matches the user's email
        // For now, we'll just remove the invitation
        _teamRepository.RemoveInvitation(invitation);
        await _teamRepository.SaveChangesAsync(ct);

        _logger.LogInformation("Invitation {InvitationId} was declined", invitationId);

        return Result.Success();
    }

    public async Task<bool> CanAccessTeamAsync(int teamId, int userId, CancellationToken ct = default)
    {
        return await _teamRepository.IsMemberAsync(teamId, userId, ct);
    }

    public async Task<bool> CanManageMembersAsync(int teamId, int userId, CancellationToken ct = default)
    {
        return await _teamRepository.HasRoleOrHigherAsync(teamId, userId, TeamRole.Admin, ct);
    }

    public async Task<bool> CanDeleteTeamAsync(int teamId, int userId, CancellationToken ct = default)
    {
        return await _teamRepository.HasRoleOrHigherAsync(teamId, userId, TeamRole.Owner, ct);
    }

    private static string GenerateInvitationToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}

