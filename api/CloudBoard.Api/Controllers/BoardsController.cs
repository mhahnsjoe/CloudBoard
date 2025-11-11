using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/boards")]
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
                .Include(b => b.Tasks)
                .ToListAsync();
            
            return Ok(boards);
        }

        // GET: api/projects/1/boards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Board>> GetBoard(int projectId, int id)
        {
            var board = await _context.Boards
                .Include(b => b.Tasks)
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

            return CreatedAtAction(nameof(GetBoard), 
                new { projectId, id = board.Id }, board);
        }

        // PUT: api/projects/1/boards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoard(int projectId, int id, BoardUpdateDto boardDto)
        {
            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == id && b.ProjectId == projectId);

            if (board == null)
                return NotFound();

            board.Name = boardDto.Name;
            board.Description = boardDto.Description;
            board.Type = boardDto.Type;

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
    }
}