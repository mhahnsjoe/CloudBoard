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

    // ==================== TASK DTOs ====================
    public class TaskItemCreateDto
    {
        public string Title { get; set; } = null!;
        public string Status { get; set; } = "To Do";
        public string Priority { get; set; } = "Medium";
        public TaskType Type { get; set; } = TaskType.Task;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public int BoardId { get; set; }
    }

    public class TaskItemUpdateDto
    {
        public string Title { get; set; } = null!;
        public string Status { get; set; } = "To Do";
        public string Priority { get; set; } = "Medium";
        public TaskType Type { get; set; } = TaskType.Task;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public int BoardId { get; set; }
    }
}