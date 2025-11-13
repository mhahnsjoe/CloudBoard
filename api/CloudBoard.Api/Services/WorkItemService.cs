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
            // Validate board exists
            var board = await _context.Boards.FindAsync(dto.BoardId);
            if (board == null)
                throw new InvalidOperationException($"Board {dto.BoardId} not found");

            // Validate parent relationship if specified
            WorkItem? parent = null;
            if (dto.ParentId.HasValue)
            {
                parent = await _context.WorkItems.FindAsync(dto.ParentId.Value);
                if (parent == null)
                    throw new InvalidOperationException("Parent workItem not found");

                if (parent.BoardId != dto.BoardId)
                    throw new InvalidOperationException("Parent must be on the same board");

                var validation = _validation.ValidateParentChild(parent.Type, dto.Type);
                if (!validation.IsValid)
                    throw new InvalidOperationException(validation.ErrorMessage);
            }

            var workItem = new WorkItem
            {
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
                AssignedToId = dto.AssignedToId
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
                throw new InvalidOperationException($"WorkItem {id} not found");

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
                        throw new InvalidOperationException("New parent not found");

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

            await _context.SaveChangesAsync();
            return workItem;
        }

        public async Task DeleteAsync(int id)
        {
            var workItem = await _context.WorkItems
                .Include(t => t.Children)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (workItem == null)
                throw new InvalidOperationException($"workItem {id} not found");

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
                throw new InvalidOperationException($"Item {itemId} not found");

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
                throw new NullReferenceException($"Item {sprintId} not found");

            if (workItem.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException($"Item {sprintId} is not owned by the user");

            // If sprintId is provided, verify it belongs to the same board
            if (dto.SprintId.HasValue)
            {
                var sprint = await _context.Sprints.FindAsync(dto.SprintId.Value);
                if (sprint == null)
                    throw new InvalidOperationException($"Sprint {sprintId} not found");

                if (sprint.BoardId != workItem.BoardId)
                    throw new InvalidOperationException($"Sprint must belong to the same board");
            }

            workItem.SprintId = dto.SprintId;
            await _context.SaveChangesAsync();
        }
    }
}