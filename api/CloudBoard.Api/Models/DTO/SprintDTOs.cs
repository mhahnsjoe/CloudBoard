namespace CloudBoard.Api.Models.DTO
{
    public class CreateSprintDto
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Goal { get; set; }
    }

    public class UpdateSprintDto
    {
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Goal { get; set; }
    }

    public class SprintDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Goal { get; set; }
        public SprintStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int BoardId { get; set; }
        public int TotalWorkItems { get; set; }
        public int CompletedWorkItems { get; set; }
        public decimal ProgressPercentage { get; set; }
        public decimal TotalEstimatedHours { get; set; }
        public decimal CompletedEstimatedHours { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class SprintStatsDto
    {
        public int TotalItems { get; set; }
        public int TodoCount { get; set; }
        public int InProgressCount { get; set; }
        public int DoneCount { get; set; }
        public decimal TotalEstimatedHours { get; set; }
        public decimal CompletedEstimatedHours { get; set; }
        public decimal RemainingEstimatedHours { get; set; }
    }

    public class BurndownPointDto
    {
        public DateTime Date { get; set; }
        public decimal RemainingHours { get; set; }
        public decimal IdealRemainingHours { get; set; }
    }
}