namespace CloudBoard.Api.Models;

/// <summary>
/// Represents a team that owns projects and has members.
/// </summary>
public class Team
{
    public int Id { get; set; }

    /// <summary>
    /// Display name for the team
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Optional description of the team's purpose
    /// </summary>
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created this team (for audit purposes)
    /// </summary>
    public int CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    // Navigation properties
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<TeamInvitation> Invitations { get; set; } = new List<TeamInvitation>();
}
