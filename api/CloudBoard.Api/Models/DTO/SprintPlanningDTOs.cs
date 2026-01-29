namespace CloudBoard.Api.Models.DTO;

/// <summary>
/// Context for sprint planning view - contains sprints and available backlog items
/// </summary>
public class SprintPlanningContextDto
{
    public List<SprintDto> Sprints { get; set; } = new();
    public List<WorkItemSummaryDto> BacklogItems { get; set; } = new();
    public decimal TotalBacklogHours { get; set; }
    public int TotalBacklogItems { get; set; }
}

/// <summary>
/// Lightweight work item for planning views
/// </summary>
public class WorkItemSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public decimal? EstimatedHours { get; set; }
    public int? ParentId { get; set; }
    public string? ParentTitle { get; set; }
    public int ChildCount { get; set; }
    public List<WorkItemSummaryDto> Children { get; set; } = new();
}

/// <summary>
/// Request to bulk assign work items to a sprint
/// </summary>
public class BulkSprintAssignmentDto
{
    public List<int> WorkItemIds { get; set; } = new();
}

/// <summary>
/// Result of bulk sprint operation
/// </summary>
public class BulkOperationResultDto
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Sprint capacity configuration
/// </summary>
public class SprintCapacityDto
{
    public int SprintId { get; set; }
    public decimal TotalCapacityHours { get; set; }
    public decimal AllocatedHours { get; set; }
    public decimal RemainingHours { get; set; }
    public decimal UtilizationPercentage { get; set; }
}

/// <summary>
/// Velocity data point for a single sprint
/// </summary>
public class SprintVelocityDto
{
    public int SprintId { get; set; }
    public string SprintName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal PlannedHours { get; set; }
    public decimal CompletedHours { get; set; }
    public int PlannedItems { get; set; }
    public int CompletedItems { get; set; }
    public decimal VelocityPercentage => PlannedHours > 0 ? (CompletedHours / PlannedHours) * 100 : 0;
}

/// <summary>
/// Board velocity summary
/// </summary>
public class BoardVelocityDto
{
    public int BoardId { get; set; }
    public List<SprintVelocityDto> SprintVelocities { get; set; } = new();
    public decimal AverageVelocityHours { get; set; }
    public decimal AverageVelocityItems { get; set; }
    public int TotalSprintsAnalyzed { get; set; }
}
