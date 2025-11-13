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
    }

    public class BoardUpdateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public BoardType Type { get; set; }
    }

}   