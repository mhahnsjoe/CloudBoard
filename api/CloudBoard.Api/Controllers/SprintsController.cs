using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using System.Security.Claims;

namespace CloudBoard.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class SprintsController : ControllerBase
    {
        private readonly CloudBoardContext _context;

        public SprintsController(CloudBoardContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!);
        }
        
        // GET /api/boards/{boardId}/sprints
        [HttpGet("boards/{boardId}/sprints")]
        public async Task<ActionResult<IEnumerable<SprintDto>>> GetSprints(int boardId)
        {
            var userId = GetUserId();

            // Verify board access
            var board = await _context.Boards
                .Include(b => b.Project)
                .FirstOrDefaultAsync(b => b.Id == boardId && b.Project!.OwnerId == userId);

            if (board == null)
                return NotFound("Board not found");

            var sprints = await _context.Sprints
                .Include(s => s.WorkItems)
                .Where(s => s.BoardId == boardId)
                .OrderByDescending(s => s.StartDate)
                .Select(s => new SprintDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Goal = s.Goal,
                    Status = s.Status,
                    CreatedAt = s.CreatedAt,
                    BoardId = s.BoardId,
                    TotalWorkItems = s.TotalWorkItems,
                    CompletedWorkItems = s.CompletedWorkItems,
                    ProgressPercentage = s.ProgressPercentage,
                    TotalEstimatedHours = s.TotalEstimatedHours,
                    CompletedEstimatedHours = s.CompletedEstimatedHours,
                    DaysRemaining = s.DaysRemaining
                })
                .ToListAsync();

            return Ok(sprints);
        }

        // GET /api/sprints/{id}
        [HttpGet("sprints/{id}")]
        public async Task<ActionResult<SprintDto>> GetSprint(int id)
        {
            var userId = GetUserId();

            var sprint = await _context.Sprints
                .Include(s => s.WorkItems)
                .Include(s => s.Board)
                    .ThenInclude(b => b.Project)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null)
                return NotFound();

            if (sprint.Board?.Project?.OwnerId != userId)
                return Forbid();

            return Ok(new SprintDto
            {
                Id = sprint.Id,
                Name = sprint.Name,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Goal = sprint.Goal,
                Status = sprint.Status,
                CreatedAt = sprint.CreatedAt,
                BoardId = sprint.BoardId,
                TotalWorkItems = sprint.TotalWorkItems,
                CompletedWorkItems = sprint.CompletedWorkItems,
                ProgressPercentage = sprint.ProgressPercentage,
                TotalEstimatedHours = sprint.TotalEstimatedHours,
                CompletedEstimatedHours = sprint.CompletedEstimatedHours,
                DaysRemaining = sprint.DaysRemaining
            });
        }

        // POST /api/boards/{boardId}/sprints
        [HttpPost("boards/{boardId}/sprints")]
        public async Task<ActionResult<SprintDto>> CreateSprint(int boardId, CreateSprintDto dto)
        {
            var userId = GetUserId();

            // Verify board access
            var board = await _context.Boards
                .Include(b => b.Project)
                .FirstOrDefaultAsync(b => b.Id == boardId && b.Project!.OwnerId == userId);

            if (board == null)
                return NotFound("Board not found");

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
                return BadRequest("End date must be after start date");

            var sprint = new Sprint
            {
                Name = dto.Name,
                StartDate = dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate.ToUniversalTime(),
                Goal = dto.Goal,
                BoardId = boardId,
                Status = SprintStatus.Planning
            };

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSprint), new { id = sprint.Id }, new SprintDto
            {
                Id = sprint.Id,
                Name = sprint.Name,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Goal = sprint.Goal,
                Status = sprint.Status,
                CreatedAt = sprint.CreatedAt,
                BoardId = sprint.BoardId,
                TotalWorkItems = 0,
                CompletedWorkItems = 0,
                ProgressPercentage = 0,
                TotalEstimatedHours = 0,
                CompletedEstimatedHours = 0,
                DaysRemaining = sprint.DaysRemaining
            });
        }

        // PUT /api/sprints/{id}
        [HttpPut("sprints/{id}")]
        public async Task<IActionResult> UpdateSprint(int id, UpdateSprintDto dto)
        {
            var userId = GetUserId();

            var sprint = await _context.Sprints
                .Include(s => s.Board)
                    .ThenInclude(b => b.Project)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null)
                return NotFound();

            if (sprint.Board?.Project?.OwnerId != userId)
                return Forbid();

            if (dto.Name != null)
                sprint.Name = dto.Name;

            if (dto.StartDate.HasValue)
            
                sprint.StartDate = dto.StartDate.Value.ToUniversalTime();

            if (dto.EndDate.HasValue)
            {
                if (dto.EndDate.Value <= sprint.StartDate)
                    return BadRequest("End date must be after start date");
                sprint.EndDate = dto.EndDate.Value.ToUniversalTime();
            }

            if (dto.Goal != null)
                sprint.Goal = dto.Goal;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH /api/sprints/{id}/start
        [HttpPatch("sprints/{id}/start")]
        public async Task<IActionResult> StartSprint(int id)
        {
            var userId = GetUserId();

            var sprint = await _context.Sprints
                .Include(s => s.Board)
                    .ThenInclude(b => b.Project)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null)
                return NotFound();

            if (sprint.Board?.Project?.OwnerId != userId)
                return Forbid();

            if (sprint.Status != SprintStatus.Planning)
                return BadRequest("Only sprints in Planning status can be started");

            // Check for other active sprints
            var hasActiveSprint = await _context.Sprints
                .AnyAsync(s => s.BoardId == sprint.BoardId && s.Status == SprintStatus.Active && s.Id != id);

            if (hasActiveSprint)
                return BadRequest("Another sprint is already active. Complete it first.");

            sprint.Status = SprintStatus.Active;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH /api/sprints/{id}/complete
        [HttpPatch("sprints/{id}/complete")]
        public async Task<IActionResult> CompleteSprint(int id)
        {
            var userId = GetUserId();

            var sprint = await _context.Sprints
                .Include(s => s.Board)
                    .ThenInclude(b => b.Project)
                .Include(s => s.WorkItems)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null)
                return NotFound();

            if (sprint.Board?.Project?.OwnerId != userId)
                return Forbid();

            if (sprint.Status != SprintStatus.Active)
                return BadRequest("Only active sprints can be completed");

            sprint.Status = SprintStatus.Completed;

            // Move incomplete items back to backlog
            var incompleteItems = sprint.WorkItems.Where(w => w.Status != "Done").ToList();
            foreach (var item in incompleteItems)
            {
                item.SprintId = null;
            }

            await _context.SaveChangesAsync();

            return Ok(new { movedToBacklog = incompleteItems.Count });
        }

        // DELETE /api/sprints/{id}
        [HttpDelete("sprints/{id}")]
        public async Task<IActionResult> DeleteSprint(int id)
        {
            var userId = GetUserId();

            var sprint = await _context.Sprints
                .Include(s => s.Board)
                    .ThenInclude(b => b.Project)
                .Include(s => s.WorkItems)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null)
                return NotFound();

            if (sprint.Board?.Project?.OwnerId != userId)
                return Forbid();

            // Move all items back to backlog
            foreach (var item in sprint.WorkItems)
            {
                item.SprintId = null;
            }

            _context.Sprints.Remove(sprint);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/sprints/{id}/stats
        [HttpGet("sprints/{id}/stats")]
        public async Task<ActionResult<SprintStatsDto>> GetSprintStats(int id)
        {
            var userId = GetUserId();

            var sprint = await _context.Sprints
                .Include(s => s.WorkItems)
                .Include(s => s.Board)
                    .ThenInclude(b => b.Project)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null)
                return NotFound();

            if (sprint.Board?.Project?.OwnerId != userId)
                return Forbid();

            var stats = new SprintStatsDto
            {
                TotalItems = sprint.WorkItems.Count,
                TodoCount = sprint.WorkItems.Count(w => w.Status == "To Do"),
                InProgressCount = sprint.WorkItems.Count(w => w.Status == "In Progress"),
                DoneCount = sprint.WorkItems.Count(w => w.Status == "Done"),
                TotalEstimatedHours = sprint.WorkItems.Sum(w => w.EstimatedHours ?? 0),
                CompletedEstimatedHours = sprint.WorkItems.Where(w => w.Status == "Done").Sum(w => w.EstimatedHours ?? 0),
                RemainingEstimatedHours = sprint.WorkItems.Where(w => w.Status != "Done").Sum(w => w.EstimatedHours ?? 0)
            };

            return Ok(stats);
        }

        // GET /api/sprints/{id}/burndown
        [HttpGet("sprints/{id}/burndown")]
        public async Task<ActionResult<IEnumerable<BurndownPointDto>>> GetSprintBurndown(int id)
        {
            var userId = GetUserId();

            var sprint = await _context.Sprints
                .Include(s => s.WorkItems)
                .Include(s => s.Board)
                    .ThenInclude(b => b.Project)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null)
                return NotFound();

            if (sprint.Board?.Project?.OwnerId != userId)
                return Forbid();

            var totalHours = sprint.TotalEstimatedHours;
            var sprintDuration = (sprint.EndDate.Date - sprint.StartDate.Date).Days + 1;
            var dailyIdealBurn = sprintDuration > 0 ? totalHours / sprintDuration : 0;

            var burndownData = new List<BurndownPointDto>();
            
            // Generate data points for each day
            for (int day = 0; day <= sprintDuration; day++)
            {
                var date = sprint.StartDate.Date.AddDays(day);
                
                // For simplicity, we'll calculate remaining work based on completion percentage
                // In a real system, you'd track historical state changes
                var idealRemaining = Math.Max(0, totalHours - (day * dailyIdealBurn));
                
                // Current remaining (this is simplified - actual would need historical data)
                var actualRemaining = date <= DateTime.UtcNow.Date 
                    ? totalHours - sprint.CompletedEstimatedHours
                    : totalHours;

                burndownData.Add(new BurndownPointDto
                {
                    Date = date,
                    RemainingHours = actualRemaining,
                    IdealRemainingHours = idealRemaining
                });
            }

            return Ok(burndownData);
        }
    }
}