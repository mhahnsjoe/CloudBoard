namespace CloudBoard.Api.Models
{
    /// <summary>
    /// Represents a configurable column in a board's workflow.
    /// Boards can have 1-5 columns with customizable names and categories.
    /// </summary>
    public class BoardColumn
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Column display name (e.g., "To Do", "In Progress", "Code Review")
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Zero-based position (0 = leftmost column)
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Workflow category for analytics and styling.
        /// Valid values: "To Do" (not started), "In Progress" (active), "Done" (complete)
        /// </summary>
        public string Category { get; set; } = "To Do";

        /// <summary>
        /// Audit timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Foreign key to Board
        /// </summary>
        public int BoardId { get; set; }

        /// <summary>
        /// Navigation property to parent Board
        /// </summary>
        public Board? Board { get; set; }
    }
}
