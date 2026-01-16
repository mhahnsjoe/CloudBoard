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
    }
}