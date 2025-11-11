namespace CloudBoard.Api.Models
{
    public class Board
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public BoardType Type { get; set; } = BoardType.Kanban;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Key
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        
        // Navigation
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }

    public enum BoardType
    {
        Kanban,
        Scrum,
        Backlog
    }
}