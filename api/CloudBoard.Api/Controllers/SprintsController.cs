using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using System.Security.Claims;
using CloudBoard.Api.Services;

namespace CloudBoard.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class SprintsController : ControllerBase
    {
        private readonly CloudBoardContext _context;
        private readonly ISprintService _sprintService;

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
            try
            {
                var sprints = await _sprintService.GetSprintsAsync(boardId, userId);
                return Ok(sprints);
            }
            catch(NullReferenceException ex)
            {
                return NotFound(ex.Message);
            }
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
            try
            {
            var sprintDto = _sprintService.CreateSprintAsync(userId, boardId, dto);
            return CreatedAtAction(nameof(GetSprint), new { id = sprintDto.Id }, sprintDto); 
            }
            catch(NullReferenceException ex)
            {
                return NotFound(ex);
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ex);
            }
        }

        // PUT /api/sprints/{id}
        [HttpPut("sprints/{id}")]
        public async Task<IActionResult> UpdateSprint(int id, UpdateSprintDto dto)
        {
            var userId = GetUserId();

            try
            {
                await _sprintService.UpdateSprintAsync(userId, id, dto);
                return NoContent();
            }
            catch(KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PATCH /api/sprints/{id}/start
        [HttpPatch("sprints/{id}/start")]
        public async Task<IActionResult> StartSprint(int id)
        {
            var userId = GetUserId();

            try
            {
                await _sprintService.StartSprintAsync(userId, id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PATCH /api/sprints/{id}/complete
        [HttpPatch("sprints/{id}/complete")]
        public async Task<IActionResult> CompleteSprint(int id)
        {
            var userId = GetUserId();
            try
            {
                var workItemsMovedToBacklog = await _sprintService.CompleteSprintAsync(userId, id);
                return Ok(new { movedToBacklog = workItemsMovedToBacklog });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE /api/sprints/{id}
        [HttpDelete("sprints/{id}")]
        public async Task<IActionResult> DeleteSprint(int id)
        {
            var userId = GetUserId();
            try
            {
                await _sprintService.DeleteSprintAsync(userId, id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // GET /api/sprints/{id}/stats
        [HttpGet("sprints/{id}/stats")]
        public async Task<ActionResult<SprintStatsDto>> GetSprintStats(int id)
        {
            var userId = GetUserId();
            try
            {
                var stats = _sprintService.GetSprintStatsAsync(userId, id);
                return Ok(stats);
            }
            catch(KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // GET /api/sprints/{id}/burndown
        [HttpGet("sprints/{id}/burndown")]
        public async Task<ActionResult<IEnumerable<BurndownPointDto>>> GetSprintBurndown(int id)
        {
            var userId = GetUserId();
            try
            {
                var burndownPoints = _sprintService.GetSprintBurndownAsync(userId, id);
                return Ok(burndownPoints);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}