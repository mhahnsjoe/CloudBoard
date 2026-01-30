using System.Collections.Generic;

namespace CloudBoard.Api.Models.DTO;

/// <summary>
/// Represents a row on the taskboard (PBI or Bug)
/// </summary>
public class TaskboardRowDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public decimal TotalHours { get; set; }
    public decimal CompletedHours { get; set; }
    public decimal RemainingHours { get; set; }
    public decimal ProgressPercentage { get; set; }
    public List<TaskboardTaskDto> Tasks { get; set; } = new();
}

/// <summary>
/// Represents a task card on the taskboard
/// </summary>
public class TaskboardTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public decimal? RemainingHours { get; set; }
    public int ParentId { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
}

/// <summary>
/// Complete taskboard data for a sprint
/// </summary>
public class TaskboardDto
{
    public int SprintId { get; set; }
    public string SprintName { get; set; } = null!;
    public List<string> Columns { get; set; } = new();
    public List<TaskboardRowDto> Rows { get; set; } = new();
    public decimal TotalHours { get; set; }
    public decimal CompletedHours { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
}
