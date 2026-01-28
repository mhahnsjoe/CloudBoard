using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

namespace CloudBoard.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projects/{projectId}/boards")]
    [Authorize]
    public class BoardsController : ControllerBase
    {
        private readonly CloudBoardContext _context;

        public BoardsController(CloudBoardContext context)
        {
            _context = context;
        }

        // GET: api/projects/1/boards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Board>>> GetBoards(int projectId)
        {
            var boards = await _context.Boards
                .Where(b => b.ProjectId == projectId)
                .Include(b => b.WorkItems)
                .Include(b => b.Columns.OrderBy(c => c.Order))
                .ToListAsync();

            return Ok(boards);
        }

        // GET: api/projects/1/boards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Board>> GetBoard(int projectId, int id)
        {
            var board = await _context.Boards
                .Include(b => b.WorkItems)
                .Include(b => b.Columns.OrderBy(c => c.Order))
                .FirstOrDefaultAsync(b => b.Id == id && b.ProjectId == projectId);

            if (board == null)
                return NotFound();

            return Ok(board);
        }

        // POST: api/projects/1/boards
        [HttpPost]
        public async Task<ActionResult<Board>> CreateBoard(int projectId, BoardCreateDto boardDto)
        {
            // Verify project exists
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound($"Project with id {projectId} not found");

            // Validate columns if provided
            if (boardDto.Columns != null && boardDto.Columns.Count > 0)
            {
                var validation = ValidateColumns(boardDto.Columns);
                if (validation != null)
                    return BadRequest(validation);
            }

            var board = new Board
            {
                Name = boardDto.Name,
                Description = boardDto.Description,
                Type = boardDto.Type,
                ProjectId = projectId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            // Create columns
            if (boardDto.Columns == null || boardDto.Columns.Count == 0)
            {
                // Create default columns
                foreach (var (name, order, category) in BoardConstants.DefaultColumns)
                {
                    board.Columns.Add(new BoardColumn
                    {
                        Name = name,
                        Order = order,
                        Category = category,
                        BoardId = board.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            else
            {
                // Create custom columns
                foreach (var colDto in boardDto.Columns)
                {
                    board.Columns.Add(new BoardColumn
                    {
                        Name = colDto.Name,
                        Order = colDto.Order,
                        Category = colDto.Category,
                        BoardId = board.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Reload board with columns for response
            await _context.Entry(board).Collection(b => b.Columns).LoadAsync();

            return CreatedAtAction(nameof(GetBoard),
                new { projectId, id = board.Id }, board);
        }

        // PUT: api/projects/1/boards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoard(int projectId, int id, BoardUpdateDto boardDto)
        {
            var board = await _context.Boards
                .Include(b => b.Columns)
                .FirstOrDefaultAsync(b => b.Id == id && b.ProjectId == projectId);

            if (board == null)
                return NotFound();

            board.Name = boardDto.Name;
            board.Description = boardDto.Description;
            board.Type = boardDto.Type;

            // Handle column updates if provided
            if (boardDto.Columns != null)
            {
                var validation = ValidateColumnsForUpdate(boardDto.Columns);
                if (validation != null)
                    return BadRequest(validation);

                await SyncBoardColumns(board, boardDto.Columns);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/projects/1/boards/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBoard(int projectId, int id)
        {
            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == id && b.ProjectId == projectId);

            if (board == null)
                return NotFound();

            _context.Boards.Remove(board);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==================== PRIVATE HELPER METHODS ====================

        /// <summary>
        /// Validates column configuration for create operations
        /// </summary>
        private string? ValidateColumns(List<BoardColumnCreateDto> columns)
        {
            if (columns.Count < BoardConstants.MinColumns || columns.Count > BoardConstants.MaxColumns)
                return $"Board must have between {BoardConstants.MinColumns} and {BoardConstants.MaxColumns} columns";

            // Check for duplicate names (case-insensitive)
            var duplicates = columns.GroupBy(c => c.Name.ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            if (duplicates.Any())
                return $"Duplicate column names found: {string.Join(", ", duplicates)}";

            // Validate categories
            foreach (var col in columns)
            {
                if (!BoardConstants.ValidCategories.Contains(col.Category))
                    return $"Invalid category '{col.Category}'. Valid categories are: {string.Join(", ", BoardConstants.ValidCategories)}";
            }

            // Validate order is sequential starting from 0
            var orders = columns.OrderBy(c => c.Order).Select(c => c.Order).ToList();
            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i] != i)
                    return $"Column orders must be sequential starting from 0";
            }

            return null;
        }

        /// <summary>
        /// Validates column configuration for update operations
        /// </summary>
        private string? ValidateColumnsForUpdate(List<BoardColumnDto> columns)
        {
            if (columns.Count < BoardConstants.MinColumns || columns.Count > BoardConstants.MaxColumns)
                return $"Board must have between {BoardConstants.MinColumns} and {BoardConstants.MaxColumns} columns";

            // Check for duplicate names (case-insensitive)
            var duplicates = columns.GroupBy(c => c.Name.ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            if (duplicates.Any())
                return $"Duplicate column names found: {string.Join(", ", duplicates)}";

            // Validate categories
            foreach (var col in columns)
            {
                if (!BoardConstants.ValidCategories.Contains(col.Category))
                    return $"Invalid category '{col.Category}'. Valid categories are: {string.Join(", ", BoardConstants.ValidCategories)}";
            }

            return null;
        }

        /// <summary>
        /// Synchronizes board columns with provided column list.
        /// Handles column creation, update, rename propagation, and deletion with item reassignment.
        /// </summary>
        private async Task SyncBoardColumns(Board board, List<BoardColumnDto> newColumns)
        {
            var existingColumns = board.Columns.ToList();
            var providedColumnIds = newColumns.Where(c => c.Id > 0).Select(c => c.Id).ToHashSet();

            // Find leftmost column for reassignment (lowest Order value in new set)
            var leftmostColumn = newColumns.OrderBy(c => c.Order).First();

            // Process deletions and reassignments first
            var columnsToDelete = existingColumns.Where(c => !providedColumnIds.Contains(c.Id)).ToList();
            foreach (var colToDelete in columnsToDelete)
            {
                // Reassign work items to leftmost column
                var itemsToReassign = await _context.WorkItems
                    .Where(w => w.BoardId == board.Id && w.Status == colToDelete.Name)
                    .ToListAsync();

                foreach (var item in itemsToReassign)
                {
                    item.Status = leftmostColumn.Name;
                }

                _context.BoardColumns.Remove(colToDelete);
            }

            // Process updates and renames
            foreach (var newCol in newColumns.Where(c => c.Id > 0))
            {
                var existingCol = existingColumns.FirstOrDefault(c => c.Id == newCol.Id);
                if (existingCol != null)
                {
                    // Check if name changed - need to propagate to work items
                    if (existingCol.Name != newCol.Name)
                    {
                        var itemsToUpdate = await _context.WorkItems
                            .Where(w => w.BoardId == board.Id && w.Status == existingCol.Name)
                            .ToListAsync();

                        foreach (var item in itemsToUpdate)
                        {
                            item.Status = newCol.Name;
                        }
                    }

                    // Update column properties
                    existingCol.Name = newCol.Name;
                    existingCol.Order = newCol.Order;
                    existingCol.Category = newCol.Category;
                }
            }

            // Process new columns (Id == 0 or not found)
            foreach (var newCol in newColumns.Where(c => c.Id == 0))
            {
                board.Columns.Add(new BoardColumn
                {
                    Name = newCol.Name,
                    Order = newCol.Order,
                    Category = newCol.Category,
                    BoardId = board.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }
}