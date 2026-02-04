namespace CloudBoard.Api.Models;

/// <summary>
/// Pending invitation to join a team.
/// </summary>
public class TeamInvitation
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    /// <summary>
    /// Email of the person being invited (may not have account yet)
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Role they will receive upon accepting
    /// </summary>
    public TeamRole Role { get; set; }

    /// <summary>
    /// Unique token for invitation link
    /// </summary>
    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public int InvitedById { get; set; }
    public User InvitedBy { get; set; } = null!;

    /// <summary>
    /// Null until accepted
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt && AcceptedAt == null;
}
