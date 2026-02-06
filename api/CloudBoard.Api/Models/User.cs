using Microsoft.AspNetCore.Identity;

namespace CloudBoard.Api.Models;

public class User : IdentityUser<int>
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<WorkItem> AssignedWorkItems { get; set; } = new List<WorkItem>();
    public ICollection<WorkItem> CreatedWorkItems { get; set; } = new List<WorkItem>();

    /// <summary>
    /// Teams this user is a member of
    /// </summary>
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();

    /// <summary>
    /// Teams this user created
    /// </summary>
    public ICollection<Team> CreatedTeams { get; set; } = new List<Team>();

    /// <summary>
    /// Invitations sent by this user
    /// </summary>
    public ICollection<TeamInvitation> SentInvitations { get; set; } = new List<TeamInvitation>();
}