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
        private readonly IWorkItemHistoryRepository _historyRepository;
        private readonly IWorkItemValidationService _validation;
        private readonly ILogger<WorkItemService> _logger;

        public WorkItemService(
            IWorkItemRepository workItemRepository,
            IBoardRepository boardRepository,
            IProjectRepository projectRepository,
            ISprintRepository sprintRepository,
            IWorkItemHistoryRepository historyRepository,
            IWorkItemValidationService validation,
            ILogger<WorkItemService> logger)
        {
            _workItemRepository = workItemRepository;
            _boardRepository = boardRepository;
            _projectRepository = projectRepository;
            _sprintRepository = sprintRepository;
            _historyRepository = historyRepository;
            _validation = validation;
            _logger = logger;
        }

        // Types that can be moved to boards
        private static readonly WorkItemType[] BoardAllowedTypes = 
        {
            WorkItemType.PBI,
            WorkItemType.Bug
        };

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
                BacklogOrder = backlogOrder,
                RemainingHours = dto.Status == "Done" ? 0 : (dto.RemainingHours ?? dto.EstimatedHours)
            };

            _workItemRepository.Add(workItem);
            await _workItemRepository.SaveChangesAsync();

            // Record initial state
            await TrackChange(workItem.Id, "Status", null, workItem.Status, createdById);
            if (workItem.SprintId.HasValue)
                await TrackChange(workItem.Id, "SprintId", null, workItem.SprintId.ToString(), createdById);
            if (workItem.EstimatedHours.HasValue)
                await TrackChange(workItem.Id, "EstimatedHours", null, workItem.EstimatedHours.ToString(), createdById);

            _logger.LogInformation(
                "Work item created: {WorkItemId} - {Title}",
                workItem.Id, workItem.Title);

            return workItem;
        }

        public async Task<WorkItem> UpdateAsync(int id, WorkItemUpdateDto dto, int currentUserId)
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

            // Track changes
            await TrackChange(workItem.Id, "Status", workItem.Status, dto.Status, currentUserId);
            await TrackChange(workItem.Id, "SprintId", workItem.SprintId?.ToString(), dto.SprintId?.ToString(), currentUserId);
            await TrackChange(workItem.Id, "EstimatedHours", workItem.EstimatedHours?.ToString(), dto.EstimatedHours?.ToString(), currentUserId);
            await TrackChange(workItem.Id, "RemainingHours", workItem.RemainingHours?.ToString(), dto.RemainingHours?.ToString(), currentUserId);

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
            workItem.RemainingHours = dto.Status == "Done" ? 0 : (dto.RemainingHours ?? (dto.Status != workItem.Status ? (dto.RemainingHours ?? dto.EstimatedHours ?? workItem.RemainingHours) : workItem.RemainingHours));
            workItem.ParentId = dto.ParentId;
            workItem.SprintId = dto.SprintId;

            await _workItemRepository.SaveChangesAsync();
            return workItem;
        }

        private async Task TrackChange(int workItemId, string fieldName, string? oldValue, string? newValue, int userId)
        {
            if (oldValue == newValue) return;

            var history = new WorkItemHistory
            {
                WorkItemId = workItemId,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedAt = DateTime.UtcNow,
                ChangedById = userId
            };

            _historyRepository.Add(history);
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

        public async Task MoveToBoardAsync(int workItemId, int? boardId, int? sprintId, int userId)
        {
            var workItem = await _workItemRepository.GetWithHierarchyAsync(workItemId);
            
            if (workItem == null)
                throw new KeyNotFoundException($"WorkItem {workItemId} not found");

            // Validate parent relationship
            // Create a pseudo-context to validate parent
            // If parent is not on the board, we might have an issue?
            // For now, we rely on the client to handle parent selection logic or the separate MoveToParent call.
            // But we should verify ownership.

            var project = await _projectRepository.GetByIdAsync(workItem.ProjectId);
            if (project == null || project.OwnerId != userId)
                throw new UnauthorizedAccessException("Not authorized to move this item");

            if (boardId.HasValue)
            {
                // Validate type restriction
                if (!BoardAllowedTypes.Contains(workItem.Type))
                {
                    throw new InvalidOperationException(
                        $"Only PBI and Bug items can be moved to boards. " +
                        $"{workItem.Type} items are for backlog organization only.");
                }

                var board = await _boardRepository.GetByIdAsync(boardId.Value);
                if (board == null)
                    throw new KeyNotFoundException($"Board {boardId} not found");
                
                if (board.ProjectId != workItem.ProjectId)
                    throw new InvalidOperationException("Board must belong to the same project");
                
                // Validate Sprint if provided
                if (sprintId.HasValue)
                {
                    var sprint = await _sprintRepository.GetByIdAsync(sprintId.Value);
                    if (sprint == null)
                        throw new KeyNotFoundException($"Sprint {sprintId} not found");
                    if (sprint.BoardId != boardId.Value)
                        throw new InvalidOperationException("Sprint must belong to the target board");
                }

                workItem.BoardId = boardId.Value;
                workItem.SprintId = sprintId; // Assign Sprint
                workItem.BacklogOrder = null;
                
                // Cascade: Move all child Tasks to the same board and sprint
                MoveChildrenToBoardRecursive(workItem, boardId.Value, sprintId);
                
                _logger.LogInformation(
                    "Moved WorkItem {Id} ({Type}) to Board {BoardId} (Sprint {SprintId}) with {ChildCount} children",
                    workItemId, workItem.Type, boardId.Value, sprintId?.ToString() ?? "None", CountDescendants(workItem));
            }
            else
            {
                // Move to backlog
                await ReturnToBacklogAsync(workItemId, userId);
            }

            await _workItemRepository.SaveChangesAsync();
        }

        private void MoveChildrenToBoardRecursive(WorkItem parent, int boardId, int? sprintId)
        {
            if (parent.Children == null)
                return;

            foreach (var child in parent.Children)
            {
                if (child.Type == WorkItemType.Task || child.Type == WorkItemType.Bug)
                {
                    child.BoardId = boardId;
                    child.SprintId = sprintId; // Cascade Sprint
                    child.BacklogOrder = null;
                    MoveChildrenToBoardRecursive(child, boardId, sprintId);
                }
            }
        }

        private int CountDescendants(WorkItem item)
        {
            if (item.Children == null || !item.Children.Any())
                return 0;
            
            return item.Children.Count + item.Children.Sum(c => CountDescendants(c));
        }

        /// <summary>
        /// Returns a work item to the backlog (sets BoardId to null)
        /// </summary>
        public async Task ReturnToBacklogAsync(int workItemId, int userId)
        {
            var workItem = await _workItemRepository.GetWithHierarchyAsync(workItemId);
            
            if (workItem == null)
                throw new KeyNotFoundException($"WorkItem {workItemId} not found");

            var project = await _projectRepository.GetByIdAsync(workItem.ProjectId);
            if (project == null || project.OwnerId != userId)
                throw new UnauthorizedAccessException("Not authorized to move this item");

            workItem.BoardId = null;
            workItem.SprintId = null;
            
            var maxOrder = await _workItemRepository.GetMaxBacklogOrderAsync(
                workItem.ProjectId, workItem.ParentId);
            workItem.BacklogOrder = (maxOrder ?? -100) + 100;
            
            // Cascade: Return all children to backlog
            await ReturnChildrenToBacklogRecursive(workItem, workItem.ProjectId);

            await _workItemRepository.SaveChangesAsync();
            
            _logger.LogInformation(
                "Returned WorkItem {Id} ({Type}) to backlog with {ChildCount} children",
                workItemId, workItem.Type, CountDescendants(workItem));
        }

        private async Task ReturnChildrenToBacklogRecursive(WorkItem parent, int projectId)
        {
            if (parent.Children == null || !parent.Children.Any())
                return;

            foreach (var child in parent.Children)
            {
                child.BoardId = null;
                child.SprintId = null;
                
                var maxOrder = await _workItemRepository.GetMaxBacklogOrderAsync(projectId, parent.Id);
                child.BacklogOrder = (maxOrder ?? -100) + 100;
                
                await ReturnChildrenToBacklogRecursive(child, projectId);
            }
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
        public async Task<WorkItemDetailDto> GetDetailsAsync(int workItemId, int userId)
        {
            var workItem = await _workItemRepository.GetWithFullContextAsync(workItemId);
            
            if (workItem == null)
                throw new KeyNotFoundException($"WorkItem {workItemId} not found");

            // Build ancestor chain
            var ancestors = new List<WorkItemLinkDto>();
            var current = workItem.Parent;
            while (current != null)
            {
                ancestors.Insert(0, new WorkItemLinkDto
                {
                    Id = current.Id,
                    Title = current.Title,
                    Type = current.Type.ToString(),
                    Status = current.Status,
                    BoardId = current.BoardId
                });
                // Need to load parent's parent - the current implementation of GetWithFullContextAsync might not load deep parents
                // For now, assuming the context loads what we need or we accept a shallow chain if not loaded
                // A better approach would be to use a recursive CTE or explicit query if deep hierarchy is needed
                current = current.Parent;
            }

            var children = workItem.Children?.ToList() ?? new List<WorkItem>();
            var completedChildren = children.Count(c => c.Status == "Done");
            var totalHours = (workItem.EstimatedHours ?? 0) + children.Sum(c => c.EstimatedHours ?? 0);
            var completedHours = children.Where(c => c.Status == "Done").Sum(c => c.EstimatedHours ?? 0);

            return new WorkItemDetailDto
            {
                Id = workItem.Id,
                Title = workItem.Title,
                Description = workItem.Description,
                Type = workItem.Type.ToString(),
                Status = workItem.Status,
                Priority = workItem.Priority,
                CreatedAt = workItem.CreatedAt,
                DueDate = workItem.DueDate,
                EstimatedHours = workItem.EstimatedHours,
                ActualHours = workItem.ActualHours,
                RemainingHours = workItem.RemainingHours,
                
                BoardId = workItem.BoardId,
                BoardName = workItem.Board?.Name,
                SprintId = workItem.SprintId,
                SprintName = workItem.Sprint?.Name,
                
                Parent = workItem.Parent != null ? new WorkItemLinkDto
                {
                    Id = workItem.Parent.Id,
                    Title = workItem.Parent.Title,
                    Type = workItem.Parent.Type.ToString(),
                    Status = workItem.Parent.Status,
                    BoardId = workItem.Parent.BoardId
                } : null,
                
                Ancestors = ancestors,
                
                Children = children.Select(c => new WorkItemChildDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Type = c.Type.ToString(),
                    Status = c.Status,
                    Priority = c.Priority,
                    EstimatedHours = c.EstimatedHours,
                    AssignedToId = c.AssignedToId,
                    AssignedToName = c.AssignedTo?.Name
                }).ToList(),
                
                TotalChildCount = children.Count,
                CompletedChildCount = completedChildren,
                TotalEstimatedHours = totalHours,
                CompletedHours = completedHours,
                ProgressPercentage = totalHours > 0 ? (completedHours / totalHours) * 100 : 0,
                
                AssignedToId = workItem.AssignedToId,
                AssignedToName = workItem.AssignedTo?.Name,
                CreatedById = workItem.CreatedById,
                CreatedByName = workItem.CreatedBy?.Name ?? "Unknown"
            };
        }
    }
}
