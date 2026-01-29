using System.ComponentModel.DataAnnotations;

namespace CloudBoard.Api.Models;

/// <summary>
/// Record of a change to a work item field for historical tracking
/// </summary>
public class WorkItemHistory
{
    public int Id { get; set; }
    
    [Required]
    public int WorkItemId { get; set; }
    public WorkItem WorkItem { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = null!; // "Status", "SprintId", "EstimatedHours"
    
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public int ChangedById { get; set; }
    public User ChangedBy { get; set; } = null!;
}
