using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskItemController : ControllerBase
    {
        private readonly CloudBoardContext _context;

        public TaskItemController(CloudBoardContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            return await _context.Tasks.Include(t => t.Project).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var task = await _context.Tasks.Include(t => t.Project)
                                           .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();
            return task;
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItemCreateDto newTaskItem)
        {
            var task = new TaskItem
            {
                Title = newTaskItem.Title,
                Status = newTaskItem.Status,
                Priority = newTaskItem.Priority,
                Description = newTaskItem.Description,
                DueDate = newTaskItem.DueDate,
                EstimatedHours = newTaskItem.EstimatedHours,
                ProjectId = newTaskItem.ProjectId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItemUpdateDto updatedTask)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            task.Title = updatedTask.Title;
            task.Status = updatedTask.Status;
            task.Priority = updatedTask.Priority;
            task.Description = updatedTask.Description;
            task.DueDate = updatedTask.DueDate;
            task.EstimatedHours = updatedTask.EstimatedHours;
            task.ActualHours = updatedTask.ActualHours;
            task.ProjectId = updatedTask.ProjectId;
            
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}