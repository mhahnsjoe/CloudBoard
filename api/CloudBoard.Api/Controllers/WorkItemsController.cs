using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CloudBoard.Api.Controllers
{
    /// <summary>
    /// API endpoints for work item management.
    /// Delegates business logic to service layer for clean separation.
    /// </summary>
    [ApiController]
    [Route("api/boards/{boardId}/workitems")]
    [Produces("application/json")]
    [Authorize]
    public class WorkItemController : ControllerBase
    {
        private readonly IWorkItemService _workItemService;

        public WorkItemController(IWorkItemService workItemService)
        {
            _workItemService = workItemService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!);
        }


        /// <summary>
        /// Gets all workitems for a board
        /// </summary>
        /// <param name="boardId">Board ID</param>
        /// <param name="includeChildren">Include full hierarchy</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkItem>>> GetWorkItems(
            int boardId,
            [FromQuery] bool includeChildren = false)
        {
            var workitems = await _workItemService.GetByBoardAsync(boardId, includeChildren);
            return Ok(workitems);
        }

        /// <summary>
        /// Gets workItem hierarchy roots (top-level items only)
        /// </summary>
        [HttpGet("hierarchy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WorkItem>>> GetWorkItemsHierarchy(int boardId)
        {
            var roots = await _workItemService.GetHierarchyRootsAsync(boardId);
            return Ok(roots);
        }

        /// <summary>
        /// Gets a single workItem by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkItem>> GetWorkItem(int boardId, int id)
        {
            var workItem = await _workItemService.GetByIdAsync(id, includeHierarchy: true);
            
            if (workItem == null || workItem.BoardId != boardId)
                return NotFound();

            return Ok(workItem);
        }

        /// <summary>
        /// Creates a new workItem
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WorkItem>> CreateWorkItem(
            int boardId,
            WorkItemCreateDto newWorkItem)
        {
            try
            {
                newWorkItem.BoardId = boardId; // Ensure consistency
                var userId = GetCurrentUserId();
                var workItem = await _workItemService.CreateAsync(newWorkItem, userId);
                return CreatedAtAction(nameof(GetWorkItem), new { boardId, id = workItem.Id }, workItem);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing workItem
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateWorkItem(
            int boardId,
            int id,
            WorkItemUpdateDto updatedWorkItem)
        {
            try
            {
                updatedWorkItem.BoardId = boardId; // Ensure consistency
                await _workItemService.UpdateAsync(id, updatedWorkItem);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a workItem
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteWorkItem(int boardId, int id)
        {
            try
            {
                await _workItemService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Moves a workItem to a different parent (or makes it top-level)
        /// </summary>
        [HttpPatch("{id}/move")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MoveWorkItem(
            int boardId,
            int id,
            [FromBody] MoveWorkItemRequest request)
        {
            try
            {
                await _workItemService.MoveToParentAsync(id, request.NewParentId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Gets the breadcrumb path for a workItem (all ancestors)
        /// </summary>
        [HttpGet("{id}/path")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<WorkItem>>> GetWorkItemPath(int boardId, int id)
        {
            var path = await _workItemService.GetPathToRootAsync(id);
            
            if (!path.Any())
                return NotFound();

            return Ok(path);
        }

        // PATCH /api/workitems/{id}/assign-sprint
        [HttpPatch("{id}/assign-sprint")]
        public async Task<IActionResult> AssignToSprint(int id, [FromBody] AssignSprintDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _workItemService.AssignToSprintAsync(id, dto, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
        //Get all backlog items for a project (items without a board)
        [HttpGet("/api/projects/{projectId}/backlog")]
        public async Task<ActionResult<IEnumerable<WorkItem>>> GetProjectBacklog(int projectId)
        {
            var workItems = await _workItemService.GetBacklogItemsAsync(projectId);
            return Ok(workItems);
        }

        //Move item to a board (or back to backlog)
        [HttpPatch("~/api/workitems/{id}/move-to-board")]
        public async Task<IActionResult> MoveToBoard(int id, [FromBody] MoveToBoardDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _workItemService.MoveToBoardAsync(id, dto.BoardId, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        //Create work item in project backlog (no board)
        [HttpPost("/api/projects/{projectId}/backlog")]
        public async Task<ActionResult<WorkItem>> CreateBacklogItem(
            int projectId,
            WorkItemCreateDto dto)
        {
            try
            {
                dto.BoardId = null;
                dto.ProjectId = projectId;
                var userId = GetCurrentUserId();
                var workItem = await _workItemService.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetWorkItem), new { boardId = 0, id = workItem.Id }, workItem);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPatch("/api/workitems/{id}/return-to-backlog")]
        public async Task<IActionResult> ReturnToBacklog(int id)
        {
            var userId = GetCurrentUserId();
            await _workItemService.ReturnToBacklogAsync(id, userId);
            return NoContent();
        }
    }
    

    /// <summary>
    /// Request model for moving workitems
    /// </summary>
    public class MoveWorkItemRequest
    {
        public int? NewParentId { get; set; }
    }
}