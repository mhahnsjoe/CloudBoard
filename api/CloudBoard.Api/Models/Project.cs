namespace CloudBoard.Api.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User who created this project (for audit purposes)
        /// </summary>
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        /// <summary>
        /// Team that owns this project. All team members have access.
        /// </summary>
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public ICollection<Board> Boards { get; set; } = new List<Board>();
    }
}