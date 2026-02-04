namespace CloudBoard.Api.Models.DTO
{
    // ==================== PROJECT DTOs ====================

    /// <summary>
    /// DTO for returning project data without circular references
    /// </summary>
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public List<BoardDto>? Boards { get; set; }
    }

    /// <summary>
    /// DTO for board data within a project
    /// </summary>
    public class BoardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public BoardType Type { get; set; }
        public int ProjectId { get; set; }
        public List<BoardColumnDto>? Columns { get; set; }
        public int WorkItemCount { get; set; }
    }

    public class ProjectCreateDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        /// <summary>
        /// Team that will own this project. All team members will have access.
        /// </summary>
        public int TeamId { get; set; }
    }

    public class ProjectUpdateDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
    }

    // ==================== BOARD DTOs ====================
    public class BoardCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public BoardType Type { get; set; } = BoardType.Kanban;
        public List<BoardColumnCreateDto>? Columns { get; set; }
    }

    public class BoardUpdateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public BoardType Type { get; set; }
        public List<BoardColumnDto>? Columns { get; set; }
    }

    // ==================== BOARD COLUMN DTOs ====================
    /// <summary>
    /// DTO for reading column data
    /// </summary>
    public class BoardColumnDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int Order { get; set; }
        public string Category { get; set; } = "To Do";
    }

    /// <summary>
    /// DTO for creating/updating columns (no Id needed for create)
    /// </summary>
    public class BoardColumnCreateDto
    {
        public string Name { get; set; } = null!;
        public int Order { get; set; }
        public string Category { get; set; } = "To Do";
    }

}   