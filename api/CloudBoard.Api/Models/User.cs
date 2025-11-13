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
}