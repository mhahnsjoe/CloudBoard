namespace CloudBoard.Api.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = "To Do"; // To Do / In Progress / Done
        public string Priority { get; set; } = "Medium"; // Low / Medium / High / Critical
        public string? Description { get; set; } // New field for task description
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; } // Optional due date
        public decimal? EstimatedHours { get; set; } // Estimated time to complete
        public decimal? ActualHours { get; set; } // Actual time spent
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}