namespace CloudBoard.Api.Models.DTO;

// === Response DTOs ===

public class TeamDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public int ProjectCount { get; set; }
    public TeamRole CurrentUserRole { get; set; }
}

public class TeamDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserSummaryDto CreatedBy { get; set; } = null!;
    public List<TeamMemberDto> Members { get; set; } = new();
    public List<ProjectSummaryDto> Projects { get; set; } = new();
    public List<TeamInvitationDto> PendingInvitations { get; set; } = new();
    public TeamRole CurrentUserRole { get; set; }
}

public class TeamMemberDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public TeamRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class TeamInvitationDto
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public TeamRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public UserSummaryDto InvitedBy { get; set; } = null!;
    public bool IsExpired { get; set; }
    public string? Token { get; set; }
}

public class UserSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class ProjectSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

// === Request DTOs ===

public class CreateTeamDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateTeamDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class InviteMemberDto
{
    public string Email { get; set; } = null!;
    public TeamRole Role { get; set; } = TeamRole.Member;
}

public class UpdateMemberRoleDto
{
    public TeamRole Role { get; set; }
}

public class AcceptInvitationDto
{
    public string Token { get; set; } = null!;
}
