public class TaskItemCreateDto
{
    public string Title { get; set; } = null!;
    public string Status { get; set; } = "To Do";
    public string Priority { get; set; } = "Medium";
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public int ProjectId { get; set; }
}

public class TaskItemUpdateDto
{
    public string Title { get; set; } = null!;
    public string Status { get; set; } = "To Do";
    public string Priority { get; set; } = "Medium";
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public int ProjectId { get; set; }
}

public class TaskItemReadDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = "To Do";
    public string Priority { get; set; } = "Medium";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public int ProjectId { get; set; }
}