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
        public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
        public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    }

    public enum BoardType
    {
        Kanban,
        Scrum,
        Backlog
    }
}