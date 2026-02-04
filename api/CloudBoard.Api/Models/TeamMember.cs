namespace CloudBoard.Api.Models;

/// <summary>
/// Join table for User-Team relationship with role information.
/// </summary>
public class TeamMember
{
    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public TeamRole Role { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who added this member (null if they created the team)
    /// </summary>
    public int? InvitedById { get; set; }
    public User? InvitedBy { get; set; }
}

/// <summary>
/// Role hierarchy for team members.
/// Higher values have more permissions.
/// </summary>
public enum TeamRole
{
    /// <summary>
    /// Can view and work on all team projects
    /// </summary>
    Member = 0,

    /// <summary>
    /// Can invite/remove members (except owner), manage projects
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Full control - can delete team, manage all members, transfer ownership
    /// </summary>
    Owner = 2
}
