namespace CloudBoard.Api.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // User relationship
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public ICollection<Board> Boards { get; set; } = new List<Board>();
    }
}