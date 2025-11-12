using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudBoard.Api.Models.DTO;

namespace CloudBoard.Api.Controllers
{
    [ApiController]
    [Route("api/boards/{boardId}/WorkItems")]
    public class WorkItemController : ControllerBase
    {
        private readonly CloudBoardContext _context;

        public WorkItemController(CloudBoardContext context)
        {
            _context = context;
        }

        // GET: api/boards/1/WorkItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkItem>>> GetWorkItems(int boardId)
        {
            var WorkItems = await _context.WorkItems
                .Where(t => t.BoardId == boardId)
                .Include(t => t.Board)
                .ToListAsync();
            
            return Ok(WorkItems);
        }

        // GET: api/boards/1/WorkItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkItem>> GetWorkItem(int boardId, int id)
        {
            var WorkItem = await _context.WorkItems
                .Include(t => t.Board)
                .FirstOrDefaultAsync(t => t.Id == id && t.BoardId == boardId);
            
            if (WorkItem == null) 
                return NotFound();
            
            return WorkItem;
        }

        // POST: api/boards/1/WorkItems
        [HttpPost]
        public async Task<ActionResult<WorkItem>> CreateWorkItem(int boardId, WorkItemCreateDto newWorkItem)
        {
            // Verify board exists
            var board = await _context.Boards.FindAsync(boardId);
            if (board == null)
                return NotFound($"Board with id {boardId} not found");

            var WorkItem = new WorkItem
            {
                Title = newWorkItem.Title,
                Status = newWorkItem.Status,
                Priority = newWorkItem.Priority,
                Type = newWorkItem.Type,
                Description = newWorkItem.Description,
                DueDate = newWorkItem.DueDate.HasValue 
                    ? DateTime.SpecifyKind(newWorkItem.DueDate.Value, DateTimeKind.Utc)
                    : null,
                EstimatedHours = newWorkItem.EstimatedHours,
                BoardId = boardId,
                CreatedAt = DateTime.UtcNow
            };

            _context.WorkItems.Add(WorkItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWorkItem), new { boardId, id = WorkItem.Id }, WorkItem);
        }

        // PUT: api/boards/1/WorkItems/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkItem(int boardId, int id, WorkItemUpdateDto updatedWorkItem)
        {
            var WorkItem = await _context.WorkItems
                .FirstOrDefaultAsync(t => t.Id == id && t.BoardId == boardId);
            
            if (WorkItem == null)
                return NotFound();

            WorkItem.Title = updatedWorkItem.Title;
            WorkItem.Status = updatedWorkItem.Status;
            WorkItem.Priority = updatedWorkItem.Priority;
            WorkItem.Type = updatedWorkItem.Type;
            WorkItem.Description = updatedWorkItem.Description;
            WorkItem.DueDate = updatedWorkItem.DueDate.HasValue 
                ? DateTime.SpecifyKind(updatedWorkItem.DueDate.Value, DateTimeKind.Utc)
                : null;
            WorkItem.EstimatedHours = updatedWorkItem.EstimatedHours;
            WorkItem.ActualHours = updatedWorkItem.ActualHours;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, ex.Message);
            }

            return NoContent();
        }

        // DELETE: api/boards/1/WorkItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkItem(int boardId, int id)
        {
            var WorkItem = await _context.WorkItems
                .FirstOrDefaultAsync(t => t.Id == id && t.BoardId == boardId);
            
            if (WorkItem == null) 
                return NotFound();
            
            _context.WorkItems.Remove(WorkItem);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        // Additional endpoint: GET all WorkItems (for Work Items view)
        [HttpGet("/api/WorkItems")]
        public async Task<ActionResult<IEnumerable<WorkItem>>> GetAllWorkItems()
        {
            return await _context.WorkItems.Include(t => t.Board).ToListAsync();
        }
    }
}