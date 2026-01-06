using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Models.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Services
{
    /// <summary>
    /// Implements work item business operations.
    /// Handles orchestration, validation, and data access.
    /// </summary>
    public class WorkItemService : IWorkItemService
    {
        private readonly CloudBoardContext _context;
        private readonly IWorkItemValidationService _validation;

        public WorkItemService(
            CloudBoardContext context, 
            IWorkItemValidationService validation)
        {
            _context = context;
            _validation = validation;
        }

        public async Task<WorkItem> CreateAsync(WorkItemCreateDto dto, int createdById)
        {
            //TODO: Implement better way to distinguish when a backlog item is created than using null
            if(dto.BoardId != null) //If boardId is set to null we are most likely creating a backlog item
            {
                // Validate board exists
                var board = await _context.Boards.FindAsync(dto.BoardId);
                if (board == null)
                    throw new KeyNotFoundException($"Board {dto.BoardId} not found");
                dto.ProjectId = board.ProjectId; //Ugly fix for now TODO: find a better way to send projectId? Maybe this works but it feels wrong
            }
            // Validate parent relationship if specified
            WorkItem? parent = null;
            if (dto.ParentId.HasValue)
            {
                parent = await _context.WorkItems.FindAsync(dto.ParentId.Value);
                if (parent == null)
                    throw new KeyNotFoundException("Parent workItem not found");

                if (parent.BoardId != dto.BoardId)
                    throw new InvalidOperationException("Parent must be on the same board");

                var validation = _validation.ValidateParentChild(parent.Type, dto.Type);
                if (!validation.IsValid)
                    throw new InvalidOperationException(validation.ErrorMessage);
            }

            var workItem = new WorkItem
            {
                ProjectId = dto.ProjectId,
                Title = dto.Title,
                Status = dto.Status,
                Priority = dto.Priority,
                Type = dto.Type,
                Description = dto.Description,
                DueDate = dto.DueDate.HasValue 
                    ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
                    : null,
                EstimatedHours = dto.EstimatedHours,
                BoardId = dto.BoardId,
                ParentId = dto.ParentId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createdById,
                AssignedToId = dto.AssignedToId,
                SprintId = dto.SprintId
            };

            _context.WorkItems.Add(workItem);
            await _context.SaveChangesAsync();

            return workItem;
        }

        public async Task<WorkItem> UpdateAsync(int id, WorkItemUpdateDto dto)
        {
            var workItem = await _context.WorkItems
                .Include(t => t.Parent)
                .Include(t => t.Children)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (workItem == null)
                throw new KeyNotFoundException($"WorkItem {id} not found");

            // Validate parent change
            if (dto.ParentId != workItem.ParentId)
            {
                var cycleValidation = await _validation.ValidateNoCycle(id, dto.ParentId);
                if (!cycleValidation.IsValid)
                    throw new InvalidOperationException(cycleValidation.ErrorMessage);

                if (dto.ParentId.HasValue)
                {
                    var newParent = await _context.WorkItems.FindAsync(dto.ParentId.Value);
                    if (newParent == null)
                        throw new KeyNotFoundException("New parent not found");

                    var parentValidation = _validation.ValidateParentChild(newParent.Type, workItem.Type);
                    if (!parentValidation.IsValid)
                        throw new InvalidOperationException(parentValidation.ErrorMessage);
                }
            }

            // Validate type change
            if (dto.Type != workItem.Type)
            {
                var typeValidation = _validation.ValidateTypeChange(workItem, dto.Type);
                if (!typeValidation.IsValid)
                    throw new InvalidOperationException(typeValidation.ErrorMessage);
            }

            // Update properties
            workItem.Title = dto.Title;
            workItem.Status = dto.Status;
            workItem.Priority = dto.Priority;
            workItem.Type = dto.Type;
            workItem.Description = dto.Description;
            workItem.DueDate = dto.DueDate.HasValue 
                ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc)
                : null;
            workItem.EstimatedHours = dto.EstimatedHours;
            workItem.ActualHours = dto.ActualHours;
            workItem.ParentId = dto.ParentId;
            workItem.SprintId = dto.SprintId;

            await _context.SaveChangesAsync();
            return workItem;
        }

        public async Task DeleteAsync(int id)
        {
            var workItem = await _context.WorkItems
                .Include(t => t.Children)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (workItem == null)
                throw new KeyNotFoundException($"workItem {id} not found");

            var validation = _validation.ValidateDelete(workItem);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            _context.WorkItems.Remove(workItem);
            await _context.SaveChangesAsync();
        }

        public async Task<WorkItem?> GetByIdAsync(int id, bool includeHierarchy = false)
        {
            var query = _context.WorkItems.AsQueryable();

            if (includeHierarchy)
            {
                query = query
                    .Include(t => t.Parent)
                    .Include(t => t.Children)
                        .ThenInclude(c => c.Children)
                            .ThenInclude(c => c.Children);
            }

            return await query.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<WorkItem>> GetByBoardAsync(int boardId, bool includeHierarchy = false)
        {
            var query = _context.WorkItems.Where(t => t.BoardId == boardId);

            if (includeHierarchy)
            {
                query = query
                    .Include(t => t.Children)
                        .ThenInclude(c => c.Children)
                            .ThenInclude(c => c.Children);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<WorkItem>> GetHierarchyRootsAsync(int boardId)
        {
            return await _context.WorkItems
                .Where(t => t.BoardId == boardId && t.ParentId == null)
                .Include(t => t.Children)
                    .ThenInclude(c => c.Children)
                        .ThenInclude(c => c.Children)
                .ToListAsync();
        }

        public async Task MoveToParentAsync(int itemId, int? newParentId)
        {
            var item = await _context.WorkItems.FindAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException($"Item {itemId} not found");

            var validation = await _validation.ValidateNoCycle(itemId, newParentId);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            item.ParentId = newParentId;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WorkItem>> GetPathToRootAsync(int itemId)
        {
            var item = await _context.WorkItems
                .Include(t => t.Parent)
                .FirstOrDefaultAsync(t => t.Id == itemId);

            if (item == null)
                return Enumerable.Empty<WorkItem>();

            return item.GetAncestors().Reverse().Append(item);
        }


        public async Task AssignToSprintAsync(int sprintId, AssignSprintDto dto, int userId)
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Board)
                    .ThenInclude(b => b!.Project) //TODO: Check if ! is a possible bug situation
                .FirstOrDefaultAsync(w => w.Id == sprintId);
                
            if (workItem == null)
                throw new KeyNotFoundException($"Item {sprintId} not found");

            if (workItem.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException($"Item {sprintId} is not owned by the user");

            // If sprintId is provided, verify it belongs to the same board
            if (dto.SprintId.HasValue)
            {
                var sprint = await _context.Sprints.FindAsync(dto.SprintId.Value);
                if (sprint == null)
                    throw new KeyNotFoundException($"Sprint {sprintId} not found");

                if (sprint.BoardId != workItem.BoardId)
                    throw new InvalidOperationException($"Sprint must belong to the same board");
            }

            workItem.SprintId = dto.SprintId;
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<WorkItem>> GetBacklogItemsAsync(int projectId)
        {
            // Get all work items for this project that have no board assigned
            var backlog = await _context.WorkItems
                .Where(w => w.ProjectId == projectId && w.BoardId == null)
                .ToListAsync();          
            return backlog;
        }

        public async Task MoveToBoardAsync(int workItemId, int? boardId, int userId)
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Board)
                    .ThenInclude(b => b.Project)
                .FirstOrDefaultAsync(w => w.Id == workItemId);

            if (workItem == null)
                throw new KeyNotFoundException("Work item not found");

            // Verify user owns this project
            // TODO: add project ownership verification logic

            if (boardId.HasValue)
            {
                var board = await _context.Boards.FindAsync(boardId.Value);
                if (board == null)
                    throw new KeyNotFoundException("Board not found");
            }

            workItem.BoardId = boardId;
            
            // If moving to backlog, also clear sprint assignment
            if (!boardId.HasValue)
            {
                workItem.SprintId = null;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Returns a work item to the backlog (sets BoardId to null)
        /// </summary>
        public async Task ReturnToBacklogAsync(int workItemId, int userId)
        {
            var workItem = await _context.WorkItems
                .Include(w => w.Board)
                    .ThenInclude(b => b!.Project)
                .FirstOrDefaultAsync(w => w.Id == workItemId);

            if (workItem == null)
                throw new KeyNotFoundException($"Work item {workItemId} not found");

            // Verify ownership through project
            if (workItem.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to modify this work item");

            // Clear board assignment (return to backlog)
            workItem.BoardId = null;
            
            // Also clear sprint assignment since it belongs to the board
            workItem.SprintId = null;

            await _context.SaveChangesAsync();
        }
    }
}