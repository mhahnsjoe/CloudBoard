namespace CloudBoard.Api.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = "To Do"; // To Do / In Progress / Done
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
    }
}
