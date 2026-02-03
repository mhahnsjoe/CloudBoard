namespace CloudBoard.Api.Models.DTO;

/// <summary>
/// Comprehensive work item detail view for the detail panel
/// </summary>
public class WorkItemDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public decimal? RemainingHours { get; set; }
    
    // Board/Sprint context
    public int? BoardId { get; set; }
    public string? BoardName { get; set; }
    public int? SprintId { get; set; }
    public string? SprintName { get; set; }
    
    // Parent info (may be in backlog)
    public WorkItemLinkDto? Parent { get; set; }
    
    // Ancestor chain for breadcrumbs
    public List<WorkItemLinkDto> Ancestors { get; set; } = new();
    
    // Children
    public List<WorkItemChildDto> Children { get; set; } = new();
    
    // Stats
    public int TotalChildCount { get; set; }
    public int CompletedChildCount { get; set; }
    public decimal TotalEstimatedHours { get; set; }
    public decimal CompletedHours { get; set; }
    public decimal ProgressPercentage { get; set; }
    
    // Assignee
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    
    // Creator
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = null!;
}

/// <summary>
/// Lightweight link to a work item (for parent/ancestor display)
/// </summary>
public class WorkItemLinkDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int? BoardId { get; set; }  // null = backlog
}

/// <summary>
/// Child work item summary
/// </summary>
public class WorkItemChildDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public decimal? EstimatedHours { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
}
