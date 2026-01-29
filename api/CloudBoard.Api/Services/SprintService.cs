using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Repositories;

namespace CloudBoard.Api.Services
{
    public class SprintService : ISprintService
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IBoardRepository _boardRepository;
        // TD-003: See ADR-005 - Add logging when sprint usage increases

        public SprintService(
            ISprintRepository sprintRepository,
            IBoardRepository boardRepository)
        {
            _sprintRepository = sprintRepository;
            _boardRepository = boardRepository;
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
                throw new KeyNotFoundException();

            if (sprint.Board?.Project?.OwnerId != userId)
                throw new UnauthorizedAccessException();

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
            return burndownData;
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
                DaysRemaining = sprint.DaysRemaining
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
    }
}
