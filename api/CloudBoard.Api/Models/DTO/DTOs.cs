namespace CloudBoard.Api.Models.DTO
{
    // ==================== PROJECT DTOs ====================
    public class ProjectCreateDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
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
        public string Category { get; set; } = "Proposed";
    }

    /// <summary>
    /// DTO for creating/updating columns (no Id needed for create)
    /// </summary>
    public class BoardColumnCreateDto
    {
        public string Name { get; set; } = null!;
        public int Order { get; set; }
        public string Category { get; set; } = "Proposed";
    }

}   