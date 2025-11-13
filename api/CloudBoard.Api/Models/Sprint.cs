namespace CloudBoard.Api.Models
{
    public class Sprint
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Goal { get; set; }
        public SprintStatus Status { get; set; } = SprintStatus.Planning;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Key
        public int BoardId { get; set; }
        public Board Board { get; set; } = null!;
        
        // Navigation
        public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
        
        // Computed properties
        public int TotalWorkItems => WorkItems?.Count ?? 0;
        public int CompletedWorkItems => WorkItems?.Count(w => w.Status == "Done") ?? 0;
        public decimal ProgressPercentage => TotalWorkItems == 0 ? 0 : (decimal)CompletedWorkItems / TotalWorkItems * 100;
        public decimal TotalEstimatedHours => WorkItems?.Sum(w => w.EstimatedHours ?? 0) ?? 0;
        public decimal CompletedEstimatedHours => WorkItems?.Where(w => w.Status == "Done").Sum(w => w.EstimatedHours ?? 0) ?? 0;
        public int DaysRemaining => (EndDate.Date - DateTime.UtcNow.Date).Days;
    }

    public enum SprintStatus
    {
        Planning = 0,
        Active = 1,
        Completed = 2
    }
}