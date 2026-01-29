using CloudBoard.Api.Models;

namespace CloudBoard.Api.Repositories;

public interface IWorkItemHistoryRepository : IRepository<WorkItemHistory>
{
    Task<List<WorkItemHistory>> GetBySprintAsync(int sprintId, CancellationToken ct = default);
    Task<List<WorkItemHistory>> GetByWorkItemAsync(int workItemId, CancellationToken ct = default);
}
