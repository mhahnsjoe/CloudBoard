using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudBoard.Api.Models.DTO;

namespace CloudBoard.Api.Controllers
{
    [ApiController]
    [Route("api/boards/{boardId}/tasks")]
    public class TaskItemController : ControllerBase
    {
        private readonly CloudBoardContext _context;

        public TaskItemController(CloudBoardContext context)
        {
            _context = context;
        }

        // GET: api/boards/1/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks(int boardId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.BoardId == boardId)
                .Include(t => t.Board)
                .ToListAsync();
            
            return Ok(tasks);
        }

        // GET: api/boards/1/tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int boardId, int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Board)
                .FirstOrDefaultAsync(t => t.Id == id && t.BoardId == boardId);
            
            if (task == null) 
                return NotFound();
            
            return task;
        }

        // POST: api/boards/1/tasks
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(int boardId, TaskItemCreateDto newTaskItem)
        {
            // Verify board exists
            var board = await _context.Boards.FindAsync(boardId);
            if (board == null)
                return NotFound($"Board with id {boardId} not found");

            var task = new TaskItem
            {
                Title = newTaskItem.Title,
                Status = newTaskItem.Status,
                Priority = newTaskItem.Priority,
                Type = newTaskItem.Type,
                Description = newTaskItem.Description,
                DueDate = newTaskItem.DueDate.HasValue 
                    ? DateTime.SpecifyKind(newTaskItem.DueDate.Value, DateTimeKind.Utc)
                    : null,
                EstimatedHours = newTaskItem.EstimatedHours,
                BoardId = boardId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { boardId, id = task.Id }, task);
        }

        // PUT: api/boards/1/tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int boardId, int id, TaskItemUpdateDto updatedTask)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.BoardId == boardId);
            
            if (task == null)
                return NotFound();

            task.Title = updatedTask.Title;
            task.Status = updatedTask.Status;
            task.Priority = updatedTask.Priority;
            task.Type = updatedTask.Type;
            task.Description = updatedTask.Description;
            task.DueDate = updatedTask.DueDate.HasValue 
                ? DateTime.SpecifyKind(updatedTask.DueDate.Value, DateTimeKind.Utc)
                : null;
            task.EstimatedHours = updatedTask.EstimatedHours;
            task.ActualHours = updatedTask.ActualHours;
            
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

        // DELETE: api/boards/1/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int boardId, int id)
        {
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.BoardId == boardId);
            
            if (task == null) 
                return NotFound();
            
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        // Additional endpoint: GET all tasks (for Work Items view)
        [HttpGet("/api/tasks")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetAllTasks()
        {
            return await _context.Tasks.Include(t => t.Board).ToListAsync();
        }
    }
}