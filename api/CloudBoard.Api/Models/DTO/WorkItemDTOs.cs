
namespace CloudBoard.Api.Models.DTO
{
    public class WorkItemCreateDto
    {
        public string Title { get; set; } = null!;
        public string Status { get; set; } = "To Do";
        public string Priority { get; set; } = "Medium";
        public WorkItemType Type { get; set; } = WorkItemType.Task;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public int BoardId { get; set; }
        public int? ParentId { get; set; }
    }

    public class WorkItemUpdateDto
    {
        public string Title { get; set; } = null!;
        public string Status { get; set; } = "To Do";
        public string Priority { get; set; } = "Medium";
        public WorkItemType Type { get; set; } = WorkItemType.Task;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public int BoardId { get; set; }
        public int? ParentId { get; set; }
    }
}