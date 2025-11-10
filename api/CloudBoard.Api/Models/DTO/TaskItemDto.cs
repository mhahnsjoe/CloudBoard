public class TaskItemCreateDto
{
    public string Title { get; set; } = null!;
    public string Status { get; set; } = "To Do";
    public int ProjectId { get; set; }
}

public class TaskItemUpdateDto
{
    public string Title { get; set; } = null!;
    public string Status { get; set; } = "To Do";
    public int ProjectId { get; set; }
}

public class TaskItemReadDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = "To Do";
    public int ProjectId { get; set; }
}
