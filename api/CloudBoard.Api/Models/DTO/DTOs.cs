namespace CloudBoard.Api.Models.DTO
{
    // ==================== PROJECT DTOs ====================
    public class ProjectCreateDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    public class ProjectUpdateDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    // ==================== BOARD DTOs ====================
    public class BoardCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public BoardType Type { get; set; } = BoardType.Kanban;
    }

    public class BoardUpdateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public BoardType Type { get; set; }
    }

    // ==================== WorkItem DTOs ====================
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
    }
}