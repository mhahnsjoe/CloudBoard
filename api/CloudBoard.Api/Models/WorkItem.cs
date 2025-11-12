namespace CloudBoard.Api.Models
{
    public class WorkItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = "To Do";
        public string Priority { get; set; } = "Medium";
        public WorkItemType Type { get; set; } = WorkItemType.Task;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        
        // Foreign Key - Changed from ProjectId to BoardId
        public int BoardId { get; set; }
        public Board? Board { get; set; }
    }

    public enum WorkItemType
    {
        Task,
        Bug,
        Feature,
        Epic
    }
}