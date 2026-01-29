using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Models.Extensions;
using CloudBoard.Api.Repositories;

namespace CloudBoard.Api.Services
{
    /// <summary>
    /// Implements work item business operations.
    /// Handles orchestration, validation, and data access.
    /// </summary>
    public class WorkItemService : IWorkItemService
    {
        private readonly IWorkItemRepository _workItemRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly IWorkItemValidationService _validation;
        private readonly ILogger<WorkItemService> _logger;

        public WorkItemService(
            IWorkItemRepository workItemRepository,
            IBoardRepository boardRepository,
            IProjectRepository projectRepository,
            ISprintRepository sprintRepository,
            IWorkItemValidationService validation,
            ILogger<WorkItemService> logger)
        {
            _workItemRepository = workItemRepository;
            _boardRepository = boardRepository;
            _projectRepository = projectRepository;
            _sprintRepository = sprintRepository;
            _validation = validation;
            _logger = logger;
        }

        public async Task<WorkItem> CreateAsync(WorkItemCreateDto dto, int createdById)
        {
            _logger.LogInformation(
                "Creating work item: {Title}, Type: {Type}, Board: {BoardId}, CreatedBy: {UserId}",
                dto.Title, dto.Type, dto.BoardId, createdById);

            // TD-001: See ADR-005 for backlog item creation pattern improvement
            if(dto.BoardId != null) //If boardId is set to null we are most likely creating a backlog item
            {
                // Validate board exists
                var board = await _boardRepository.GetByIdAsync(dto.BoardId.Value);
                if (board == null)
                {
                    _logger.LogWarning("Board {BoardId} not found", dto.BoardId);
                    throw new KeyNotFoundException($"Board {dto.BoardId} not found");
                }
                // TD-002: See ADR-005 for projectId assignment improvement
                dto.ProjectId = board.ProjectId;
            }
            // Validate parent relationship if specified
            WorkItem? parent = null;
            if (dto.ParentId.HasValue)
            {
                parent = await _workItemRepository.GetByIdAsync(dto.ParentId.Value);
                if (parent == null)
                    throw new KeyNotFoundException("Parent workItem not found");

                if (parent.BoardId != dto.BoardId)
                    throw new InvalidOperationException("Parent must be on the same board");

                var validation = _validation.ValidateParentChild(parent.Type, dto.Type);
                if (!validation.IsValid)
                    throw new InvalidOperationException(validation.ErrorMessage);
            }

            // If creating a backlog item, assign next BacklogOrder
            int? backlogOrder = null;
            if (dto.BoardId == null)
            {
                // Get the highest BacklogOrder for this project and parent level
                var maxOrder = await _workItemRepository.GetMaxBacklogOrderAsync(dto.ProjectId, dto.ParentId);
                backlogOrder = (maxOrder ?? -100) + 100; // Start at 0, increment by 100
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
                SprintId = dto.SprintId,
                BacklogOrder = backlogOrder
            };

            _workItemRepository.Add(workItem);
            await _workItemRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Work item created: {WorkItemId} - {Title}",
                workItem.Id, workItem.Title);

            return workItem;
        }

        public async Task<WorkItem> UpdateAsync(int id, WorkItemUpdateDto dto)
        {
            var workItem = await _workItemRepository.GetWithHierarchyAsync(id);

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
                    var newParent = await _workItemRepository.GetByIdAsync(dto.ParentId.Value);
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

            await _workItemRepository.SaveChangesAsync();
            return workItem;
        }

        public async Task DeleteAsync(int id)
        {
            var workItem = await _workItemRepository.GetWithHierarchyAsync(id);

            if (workItem == null)
                throw new KeyNotFoundException($"workItem {id} not found");

            var validation = _validation.ValidateDelete(workItem);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            _workItemRepository.Remove(workItem);
            await _workItemRepository.SaveChangesAsync();
        }

        public async Task<WorkItem?> GetByIdAsync(int id, bool includeHierarchy = false)
        {
            if (includeHierarchy)
            {
                return await _workItemRepository.GetWithHierarchyAsync(id);
            }
            return await _workItemRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<WorkItem>> GetByBoardAsync(int boardId, bool includeHierarchy = false)
        {
            return await _workItemRepository.GetByBoardAsync(boardId);
        }

        public async Task<IEnumerable<WorkItem>> GetHierarchyRootsAsync(int boardId)
        {
            return await _workItemRepository.GetRootsByBoardAsync(boardId);
        }

        public async Task MoveToParentAsync(int itemId, int? newParentId)
        {
            var item = await _workItemRepository.GetByIdAsync(itemId);
            if (item == null)
                throw new KeyNotFoundException($"Item {itemId} not found");

            var validation = await _validation.ValidateNoCycle(itemId, newParentId);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage);

            item.ParentId = newParentId;
            await _workItemRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<WorkItem>> GetPathToRootAsync(int itemId)
        {
            var item = await _workItemRepository.GetWithHierarchyAsync(itemId);

            if (item == null)
                return Enumerable.Empty<WorkItem>();

            return item.GetAncestors().Reverse().Append(item);
        }


        public async Task AssignToSprintAsync(int workItemId, AssignSprintDto dto, int userId)
        {
            var workItem = await _workItemRepository.GetWithFullContextAsync(workItemId);

            if (workItem == null)
                throw new KeyNotFoundException($"Item {workItemId} not found");

            if (workItem.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException($"Item {workItemId} is not owned by the user");

            // If sprintId is provided, verify it belongs to the same board
            if (dto.SprintId.HasValue)
            {
                var sprint = await _sprintRepository.GetByIdAsync(dto.SprintId.Value);
                if (sprint == null)
                    throw new KeyNotFoundException($"Sprint {dto.SprintId} not found");

                if (sprint.BoardId != workItem.BoardId)
                    throw new InvalidOperationException($"Sprint must belong to the same board");
            }

            workItem.SprintId = dto.SprintId;
            await _workItemRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<WorkItem>> GetBacklogItemsAsync(int projectId, CancellationToken ct = default)
        {
            return await _workItemRepository.GetBacklogAsync(projectId, ct);
        }

        public async Task MoveToBoardAsync(int workItemId, int? boardId, int userId)
        {
            var workItem = await _workItemRepository.GetWithFullContextAsync(workItemId);

            if (workItem == null)
                throw new KeyNotFoundException("Work item not found");

            // TD-004: See ADR-005 for project ownership verification logic

            if (boardId.HasValue)
            {
                var board = await _boardRepository.GetByIdAsync(boardId.Value);
                if (board == null)
                    throw new KeyNotFoundException("Board not found");
            }

            workItem.BoardId = boardId;

            // If moving to backlog, also clear sprint assignment
            if (!boardId.HasValue)
            {
                workItem.SprintId = null;
            }

            await _workItemRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Returns a work item to the backlog (sets BoardId to null)
        /// </summary>
        public async Task ReturnToBacklogAsync(int workItemId, int userId)
        {
            var workItem = await _workItemRepository.GetWithFullContextAsync(workItemId);

            if (workItem == null)
                throw new KeyNotFoundException($"Work item {workItemId} not found");

            // Verify ownership through project
            if (workItem.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to modify this work item");

            // Clear board assignment (return to backlog)
            workItem.BoardId = null;

            // Also clear sprint assignment since it belongs to the board
            workItem.SprintId = null;

            await _workItemRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Reorder backlog items by setting their BacklogOrder values
        /// </summary>
        public async Task ReorderBacklogItemsAsync(int projectId, List<Controllers.ItemOrder> itemOrders, int userId)
        {
            // Verify project exists and user has access
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new KeyNotFoundException($"Project {projectId} not found");

            if (project.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to modify this project");

            // Get all items to be reordered
            var itemIds = itemOrders.Select(io => io.ItemId).ToList();
            var items = await _workItemRepository.GetBacklogItemsByIdsAsync(projectId, itemIds);

            // Verify all items exist and belong to the backlog
            if (items.Count != itemIds.Count)
                throw new InvalidOperationException("Some items were not found or don't belong to the backlog");

            // Update BacklogOrder for each item
            foreach (var itemOrder in itemOrders)
            {
                var item = items.First(i => i.Id == itemOrder.ItemId);
                item.BacklogOrder = itemOrder.Order;
            }

            await _workItemRepository.SaveChangesAsync();
        }
    }
}
