using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace CloudBoard.Api.Services
{
    public class SprintService : ISprintService
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IWorkItemRepository _workItemRepository;
        private readonly IWorkItemHistoryRepository _historyRepository;

        private static readonly WorkItemType[] AllowedSprintTypes = 
        {
            WorkItemType.PBI,
            WorkItemType.Task,
            WorkItemType.Bug
        };

        public SprintService(
            ISprintRepository sprintRepository,
            IBoardRepository boardRepository,
            IWorkItemRepository workItemRepository,
            IWorkItemHistoryRepository historyRepository)
        {
            _sprintRepository = sprintRepository;
            _boardRepository = boardRepository;
            _workItemRepository = workItemRepository;
            _historyRepository = historyRepository;
        }

        public async Task<SprintPlanningContextDto> GetSprintPlanningContextAsync(
            int boardId, 
            int userId, 
            CancellationToken cancellationToken = default)
        {
            // Verify board access
            var board = await _boardRepository.GetWithProjectAsync(boardId, cancellationToken);
            if (board == null || board.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException("Board not found or access denied");

            // Get all sprints for the board
            var sprints = await _sprintRepository.GetByBoardAsync(boardId, cancellationToken);

            // Get backlog items from the project (items without board assignment)
            var backlogItems = await _workItemRepository.GetBacklogAsync(board.ProjectId, cancellationToken);
            
            // Only show root items (no parent) to avoid double-counting capacity
            var rootItems = backlogItems.Where(w => w.ParentId == null).ToList();
            
            var unassignedItems = rootItems
                .Select(w =>
                {
                    var children = backlogItems.Where(c => c.ParentId == w.Id).ToList();
                    var totalHours = (w.EstimatedHours ?? 0) + children.Sum(c => c.EstimatedHours ?? 0);
                    
                    return new WorkItemSummaryDto
                    {
                        Id = w.Id,
                        Title = w.Title,
                        Type = w.Type.ToString(),
                        Status = w.Status,
                        Priority = w.Priority,
                        EstimatedHours = totalHours,
                        ParentId = w.ParentId,
                        ParentTitle = w.Parent?.Title,
                        ChildCount = children.Count,
                        Children = children.Select(c => new WorkItemSummaryDto
                        {
                            Id = c.Id,
                            Title = c.Title,
                            Type = c.Type.ToString(),
                            Status = c.Status,
                            Priority = c.Priority,
                            EstimatedHours = c.EstimatedHours,
                            ParentId = c.ParentId,
                            ParentTitle = w.Title,
                            ChildCount = 0,
                            Children = new()
                        }).ToList()
                    };
                })
                .ToList();

            return new SprintPlanningContextDto
            {
                Sprints = sprints.Select(MapToDto).ToList(),
                BacklogItems = unassignedItems,
                TotalBacklogHours = unassignedItems.Sum(i => i.EstimatedHours ?? 0),
                TotalBacklogItems = unassignedItems.Count
            };
        }

        public async Task<BulkOperationResultDto> BulkAssignToSprintAsync(
            int sprintId, 
            List<int> workItemIds, 
            int userId, 
            CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(sprintId, cancellationToken);
            if (sprint == null)
                throw new KeyNotFoundException("Sprint not found");

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            if (sprint.Status == SprintStatus.Completed)
                throw new InvalidOperationException("Cannot add items to a completed sprint");

            var result = new BulkOperationResultDto();

            foreach (var workItemId in workItemIds)
            {
                try
                {
                    var workItem = await _workItemRepository.GetByIdAsync(workItemId, cancellationToken);
                    if (workItem == null)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Work item {workItemId} not found");
                        continue;
                    }

                    // Validate: item must be from project backlog OR same board
                    if (workItem.BoardId.HasValue && workItem.BoardId != sprint.BoardId)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Work item {workItemId} belongs to different board");
                        continue;
                    }

                    // Validate: item must belong to same project
                    if (workItem.ProjectId != sprint.Board!.ProjectId)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Work item {workItemId} belongs to different project");
                        continue;
                    }

                    if (!AllowedSprintTypes.Contains(workItem.Type))
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Work item {workItemId} type '{workItem.Type}' cannot be added to sprints. Only PBI, Task, and Bug allowed.");
                        continue;
                    }

                    // If from backlog, assign to board
                    if (!workItem.BoardId.HasValue)
                    {
                        await TrackChange(workItemId, "BoardId", null, sprint.BoardId.ToString(), userId);
                        workItem.BoardId = sprint.BoardId;
                    }

                    await TrackChange(workItemId, "SprintId", workItem.SprintId?.ToString(), sprintId.ToString(), userId);
                    workItem.SprintId = sprintId;
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Failed to assign item {workItemId}: {ex.Message}");
                }
            }

            await _sprintRepository.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<BulkOperationResultDto> BulkUnassignFromSprintAsync(
            int sprintId, 
            List<int> workItemIds, 
            int userId, 
            CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(sprintId, cancellationToken);
            if (sprint == null)
                throw new KeyNotFoundException("Sprint not found");

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            var result = new BulkOperationResultDto();

            foreach (var workItemId in workItemIds)
            {
                try
                {
                    var workItem = sprint.WorkItems.FirstOrDefault(w => w.Id == workItemId);
                    if (workItem == null)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Work item {workItemId} not in this sprint");
                        continue;
                    }

                    await TrackChange(workItemId, "SprintId", workItem.SprintId?.ToString(), null, userId);
                    workItem.SprintId = null;
                    
                    // Return item to project backlog by clearing BoardId
                    if (workItem.BoardId.HasValue)
                    {
                        await TrackChange(workItemId, "BoardId", workItem.BoardId.ToString(), null, userId);
                        workItem.BoardId = null;
                    }
                    
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Failed to unassign item {workItemId}: {ex.Message}");
                }
            }

            await _sprintRepository.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<BoardVelocityDto> GetBoardVelocityAsync(
            int boardId, 
            int userId, 
            int sprintCount = 6, 
            CancellationToken cancellationToken = default)
        {
            var board = await _boardRepository.GetWithProjectAsync(boardId, cancellationToken);
            if (board == null || board.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException("Board not found or access denied");

            var completedSprints = (await _sprintRepository.GetByBoardAsync(boardId, cancellationToken))
                .Where(s => s.Status == SprintStatus.Completed)
                .OrderByDescending(s => s.EndDate)
                .Take(sprintCount)
                .ToList();

            var velocities = completedSprints.Select(s => new SprintVelocityDto
            {
                SprintId = s.Id,
                SprintName = s.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                PlannedHours = s.TotalEstimatedHours,
                CompletedHours = s.CompletedEstimatedHours,
                PlannedItems = s.TotalWorkItems,
                CompletedItems = s.CompletedWorkItems
            }).ToList();

            return new BoardVelocityDto
            {
                BoardId = boardId,
                SprintVelocities = velocities,
                AverageVelocityHours = velocities.Any() ? velocities.Average(v => v.CompletedHours) : 0,
                AverageVelocityItems = velocities.Any() ? (decimal)velocities.Average(v => v.CompletedItems) : 0,
                TotalSprintsAnalyzed = velocities.Count
            };
        }

        public async Task<SprintCapacityDto> GetSprintCapacityAsync(
            int sprintId, 
            int userId, 
            CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(sprintId, cancellationToken);
            if (sprint == null)
                throw new KeyNotFoundException("Sprint not found");

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            var allocatedHours = sprint.TotalEstimatedHours;
            var capacityHours = sprint.CapacityHours ?? 0;

            return new SprintCapacityDto
            {
                SprintId = sprintId,
                TotalCapacityHours = capacityHours,
                AllocatedHours = allocatedHours,
                RemainingHours = Math.Max(0, capacityHours - allocatedHours),
                UtilizationPercentage = capacityHours > 0 ? (allocatedHours / capacityHours) * 100 : 0
            };
        }

        public async Task SetSprintCapacityAsync(
            int sprintId, 
            decimal capacityHours, 
            int userId, 
            CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(sprintId, cancellationToken);
            if (sprint == null)
                throw new KeyNotFoundException("Sprint not found");

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            if (capacityHours < 0)
                throw new InvalidOperationException("Capacity cannot be negative");

            sprint.CapacityHours = capacityHours;
            await _sprintRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateRetrospectiveAsync(
            int sprintId, 
            string retrospective, 
            int userId, 
            CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(sprintId, cancellationToken);
            if (sprint == null)
                throw new KeyNotFoundException("Sprint not found");

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            sprint.Retrospective = retrospective;
            sprint.RetrospectiveDate = DateTime.UtcNow;
            await _sprintRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task<SprintDto> GetSprintAsync(int sprintId, int userId, CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(sprintId, cancellationToken);

            if (sprint == null)
                throw new KeyNotFoundException("Sprint not found");

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException("Unauthorized access to sprint");

            return MapToDto(sprint);
        }

        public async Task<List<SprintDto>> GetSprintsAsync(int boardId, int userId, CancellationToken cancellationToken = default)
        {
            // Verify board access
            var board = await _boardRepository.GetWithProjectAsync(boardId, cancellationToken);

            if (board == null || board.Project?.OwnerId != userId)
                throw new NullReferenceException("Board not found");

            var sprints = await _sprintRepository.GetByBoardAsync(boardId, cancellationToken);

            return sprints.Select(MapToDto).ToList();
        }

        public async Task<SprintDto> CreateSprintAsync(int userId, int boardId, CreateSprintDto dto, CancellationToken cancellationToken = default)
        {
            // Verify board access
            var board = await _boardRepository.GetWithProjectAsync(boardId, cancellationToken);

            if (board == null || board.Project?.OwnerId != userId)
                throw new NullReferenceException("Board not found");

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
                throw new InvalidOperationException("End date must be after start date");

            var sprint = new Sprint
            {
                Name = dto.Name,
                StartDate = dto.StartDate.ToUniversalTime(),
                EndDate = dto.EndDate.ToUniversalTime(),
                Goal = dto.Goal,
                BoardId = boardId,
                Status = SprintStatus.Planning
            };

            _sprintRepository.Add(sprint);
            await _sprintRepository.SaveChangesAsync(cancellationToken);

            return new SprintDto
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
            };
        }

        public async Task UpdateSprintAsync(int userId, int id, UpdateSprintDto dto, CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(id, cancellationToken);

            if (sprint == null)
                throw new KeyNotFoundException();

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            if (dto.Name != null)
                sprint.Name = dto.Name;

            if (dto.StartDate.HasValue)
                sprint.StartDate = dto.StartDate.Value.ToUniversalTime();

            if (dto.EndDate.HasValue)
            {
                if (dto.EndDate.Value <= sprint.StartDate)
                    throw new InvalidOperationException("End date must be after start date");
                sprint.EndDate = dto.EndDate.Value.ToUniversalTime();
            }

            if (dto.Goal != null)
                sprint.Goal = dto.Goal;

            await _sprintRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task StartSprintAsync(int userId, int id, CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(id, cancellationToken);

            if (sprint == null)
                throw new KeyNotFoundException();

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            if (sprint.Status != SprintStatus.Planning)
                throw new InvalidOperationException("Only sprints in Planning status can be started");

            // Check for other active sprints
            var hasActiveSprint = await _sprintRepository.HasActiveSprintAsync(sprint.BoardId, id, cancellationToken);

            if (hasActiveSprint)
                throw new InvalidOperationException("Another sprint is already active. Complete it first.");

            sprint.Status = SprintStatus.Active;
            await _sprintRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> CompleteSprintAsync(int userId, int id, CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(id, cancellationToken);

            if (sprint == null)
                throw new KeyNotFoundException();

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            if (sprint.Status != SprintStatus.Active)
                throw new InvalidOperationException("Only active sprints can be completed");

            sprint.Status = SprintStatus.Completed;

            var incompleteWorkItemsCount = MoveIncompleteWorkItemsToBacklog(sprint);

            await _sprintRepository.SaveChangesAsync(cancellationToken);
            return incompleteWorkItemsCount;
        }

        public async Task DeleteSprintAsync(int userId, int id, CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(id, cancellationToken);

            if (sprint == null)
                throw new KeyNotFoundException();

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            // Move all items back to backlog
            foreach (var item in sprint.WorkItems)
            {
                item.SprintId = null;
            }

            _sprintRepository.Remove(sprint);
            await _sprintRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task<SprintStatsDto> GetSprintStatsAsync(int userId, int id, CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(id, cancellationToken);

            if (sprint == null)
                throw new KeyNotFoundException();

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

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
            return stats;
        }

        public async Task<List<BurndownPointDto>> GetSprintBurndownAsync(int userId, int id, CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(id, cancellationToken);

            if (sprint == null)
                throw new KeyNotFoundException("Sprint not found");

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            var history = await _historyRepository.GetBySprintAsync(id, cancellationToken);
            
            var totalHours = sprint.TotalEstimatedHours;
            var sprintDuration = (sprint.EndDate.Date - sprint.StartDate.Date).Days + 1;
            var dailyIdealBurn = sprintDuration > 0 ? totalHours / sprintDuration : 0;

            var burndownData = new List<BurndownPointDto>();

            // For history calculation, we need the "current" state of all items ever involved
            var itemsByHistory = history.Select(h => h.WorkItem).DistinctBy(w => w.Id).ToList();
            
            // Items currently in sprint might not have history yet
            var currentItemsInSprint = await _workItemRepository.GetBySprintAsync(id, cancellationToken);
            
            var allRelevantItems = itemsByHistory.UnionBy(currentItemsInSprint, w => w.Id).ToList();

            for (int day = 0; day <= sprintDuration; day++)
            {
                var date = sprint.StartDate.Date.AddDays(day);
                var endOfDay = date.Date.AddDays(1).AddSeconds(-1);

                decimal actualRemaining = 0;

                if (date > DateTime.UtcNow.Date)
                {
                    // For future dates, use current remaining hours
                    actualRemaining = currentItemsInSprint
                        .Where(w => w.Status != "Done")
                        .Sum(w => w.EstimatedHours ?? 0);
                }
                else
                {
                    // Calculate historical remaining hours
                    foreach (var item in allRelevantItems)
                    {
                        var state = GetItemStateAt(item, history, endOfDay);
                        if (state.SprintId == sprint.Id && state.Status != "Done")
                        {
                            actualRemaining += state.Estimate;
                        }
                    }
                }

                burndownData.Add(new BurndownPointDto
                {
                    Date = date,
                    RemainingHours = actualRemaining,
                    IdealRemainingHours = Math.Max(0, totalHours - (day * dailyIdealBurn))
                });
            }

            return burndownData;
        }

        private (int? SprintId, string Status, decimal Estimate) GetItemStateAt(
            WorkItem item, 
            List<WorkItemHistory> history, 
            DateTime endOfDay)
        {
            // Start with current state
            int? sprintId = item.SprintId;
            string status = item.Status;
            decimal estimate = item.EstimatedHours ?? 0;

            // Undo changes that happened after endOfDay
            var changesToUndo = history
                .Where(h => h.WorkItemId == item.Id && h.ChangedAt > endOfDay)
                .OrderByDescending(h => h.ChangedAt);

            foreach (var change in changesToUndo)
            {
                if (change.FieldName == "Status") 
                    status = change.OldValue ?? status;
                else if (change.FieldName == "SprintId") 
                    sprintId = string.IsNullOrEmpty(change.OldValue) ? null : int.Parse(change.OldValue);
                else if (change.FieldName == "EstimatedHours") 
                    estimate = string.IsNullOrEmpty(change.OldValue) ? 0 : decimal.Parse(change.OldValue);
            }

            return (sprintId, status, estimate);
        }

        private static SprintDto MapToDto(Sprint sprint)
        {
            return new SprintDto
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
                DaysRemaining = sprint.DaysRemaining,
                CapacityHours = sprint.CapacityHours,
                CapacityUtilization = sprint.CapacityUtilization,
                Retrospective = sprint.Retrospective,
                RetrospectiveDate = sprint.RetrospectiveDate
            };
        }

        private static int MoveIncompleteWorkItemsToBacklog(Sprint sprint)
        {
            var incompleteItems = sprint.WorkItems.Where(w => w.Status != "Done").ToList();
            foreach (var item in incompleteItems)
            {
                item.SprintId = null;
            }
            return incompleteItems.Count;
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

        public async Task<TaskboardDto> GetTaskboardAsync(
            int sprintId, 
            int userId, 
            CancellationToken cancellationToken = default)
        {
            var sprint = await _sprintRepository.GetWithFullContextAsync(sprintId, cancellationToken);
            
            if (sprint == null)
                throw new KeyNotFoundException("Sprint not found");

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

            // Get board columns for status headers
            var columns = sprint.Board.Columns
                .OrderBy(c => c.Order)
                .Select(c => c.Name)
                .ToList();

            if (!columns.Any())
                columns = new List<string> { "To Do", "In Progress", "Done" };

            // Get all sprint work items
            var sprintItems = await _workItemRepository.GetBySprintAsync(sprintId, cancellationToken);

            // Filter to PBI and standalone Bug as row headers
            var parentItems = sprintItems
                .Where(w => w.Type == WorkItemType.PBI || 
                           (w.Type == WorkItemType.Bug && !w.ParentId.HasValue))
                .ToList();

            // Build taskboard rows
            var rows = parentItems.Select(parent =>
            {
                var childTasks = sprintItems
                    .Where(w => w.ParentId == parent.Id && 
                               (w.Type == WorkItemType.Task || w.Type == WorkItemType.Bug))
                    .ToList();

                var totalHours = childTasks.Sum(t => t.EstimatedHours ?? 0);
                var completedHours = childTasks
                    .Where(t => t.Status == "Done")
                    .Sum(t => t.EstimatedHours ?? 0);
                
                var remainingHours = childTasks.Sum(t => t.RemainingHours ?? 0);

                if (parent.Type == WorkItemType.PBI || (parent.Type == WorkItemType.Bug && !parent.ParentId.HasValue))
                {
                    totalHours += parent.EstimatedHours ?? 0;
                    if (parent.Status == "Done")
                        completedHours += parent.EstimatedHours ?? 0;
                    
                    remainingHours += parent.RemainingHours ?? 0;
                }

                return new TaskboardRowDto
                {
                    Id = parent.Id,
                    Title = parent.Title,
                    Type = parent.Type.ToString(),
                    Status = parent.Status,
                    Priority = parent.Priority,
                    TotalHours = totalHours,
                    CompletedHours = completedHours,
                    RemainingHours = remainingHours,
                    ProgressPercentage = totalHours > 0 ? (completedHours / totalHours) * 100 : 0,
                    Tasks = childTasks.Select(task => new TaskboardTaskDto
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Type = task.Type.ToString(),
                        Status = task.Status,
                        Priority = task.Priority,
                        EstimatedHours = task.EstimatedHours,
                        ActualHours = task.ActualHours,
                        RemainingHours = task.RemainingHours,
                        ParentId = parent.Id,
                        AssignedToId = task.AssignedToId,
                        AssignedToName = task.AssignedTo?.Name
                    }).ToList()
                };
            }).ToList();

            // Handle orphan tasks (tasks without parent but in sprint)
            var orphanTasks = sprintItems
                .Where(w => w.Type == WorkItemType.Task && !w.ParentId.HasValue)
                .ToList();

            if (orphanTasks.Any())
            {
                rows.Add(new TaskboardRowDto
                {
                    Id = 0,
                    Title = "Unparented Tasks",
                    Type = "Unparented",
                    Status = "N/A",
                    Priority = "N/A",
                    TotalHours = orphanTasks.Sum(t => t.EstimatedHours ?? 0),
                    CompletedHours = orphanTasks.Where(t => t.Status == "Done").Sum(t => t.EstimatedHours ?? 0),
                    RemainingHours = orphanTasks.Sum(t => t.RemainingHours ?? 0),
                    ProgressPercentage = 0,
                    Tasks = orphanTasks.Select(task => new TaskboardTaskDto
                    {
                        Id = task.Id,
                        Title = task.Title,
                        Type = task.Type.ToString(),
                        Status = task.Status,
                        Priority = task.Priority,
                        EstimatedHours = task.EstimatedHours,
                        ActualHours = task.ActualHours,
                        RemainingHours = task.RemainingHours,
                        ParentId = 0,
                        AssignedToId = task.AssignedToId,
                        AssignedToName = task.AssignedTo?.Name
                    }).ToList()
                });
            }

            return new TaskboardDto
            {
                SprintId = sprintId,
                SprintName = sprint.Name,
                Columns = columns,
                Rows = rows,
                TotalHours = rows.Sum(r => r.TotalHours),
                CompletedHours = rows.Sum(r => r.CompletedHours),
                TotalTasks = rows.Sum(r => r.Tasks.Count),
                CompletedTasks = rows.Sum(r => r.Tasks.Count(t => t.Status == "Done"))
            };
        }
    }
}
