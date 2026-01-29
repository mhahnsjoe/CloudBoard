namespace CloudBoard.Api.Services
{
    using CloudBoard.Api.Models;
    using CloudBoard.Api.Models.DTO;
    public interface ISprintService
    {
        Task<SprintDto> GetSprintAsync(int sprintId, int userId, CancellationToken cancellationToken = default);
        Task<List<SprintDto>> GetSprintsAsync(int boardId, int userId, CancellationToken cancellationToken = default);
        Task<SprintDto> CreateSprintAsync(int userId, int boardId, CreateSprintDto dto, CancellationToken cancellationToken = default);
        Task UpdateSprintAsync(int userId, int id, UpdateSprintDto dto, CancellationToken cancellationToken = default);
        Task StartSprintAsync(int userId, int id, CancellationToken cancellationToken = default);
        Task<int> CompleteSprintAsync(int userId, int id, CancellationToken cancellationToken = default);
        Task DeleteSprintAsync(int userId, int id, CancellationToken cancellationToken = default);
        Task<SprintStatsDto> GetSprintStatsAsync (int userId, int id, CancellationToken cancellationToken = default);
        Task<List<BurndownPointDto>> GetSprintBurndownAsync (int userId, int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets sprint planning context with backlog items available for assignment
        /// </summary>
        Task<SprintPlanningContextDto> GetSprintPlanningContextAsync(
            int boardId, 
            int userId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk assigns work items to a sprint
        /// </summary>
        Task<BulkOperationResultDto> BulkAssignToSprintAsync(
            int sprintId, 
            List<int> workItemIds, 
            int userId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bulk removes work items from a sprint (returns to backlog)
        /// </summary>
        Task<BulkOperationResultDto> BulkUnassignFromSprintAsync(
            int sprintId, 
            List<int> workItemIds, 
            int userId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets velocity data for a board's completed sprints
        /// </summary>
        Task<BoardVelocityDto> GetBoardVelocityAsync(
            int boardId, 
            int userId, 
            int sprintCount = 6, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets capacity information for a sprint
        /// </summary>
        Task<SprintCapacityDto> GetSprintCapacityAsync(
            int sprintId, 
            int userId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the capacity for a sprint
        /// </summary>
        Task SetSprintCapacityAsync(
            int sprintId, 
            decimal capacityHours, 
            int userId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the sprint retrospective
        /// </summary>
        Task UpdateRetrospectiveAsync(
            int sprintId, 
            string retrospective, 
            int userId, 
            CancellationToken cancellationToken = default);
    }
}