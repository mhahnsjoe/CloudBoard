namespace CloudBoard.Api.Services
{
    using CloudBoard.Api.Models;
    using CloudBoard.Api.Models.DTO;

    /// <summary>
    /// Business logic for work item operations.
    /// Separates concerns from controller layer.
    /// </summary>
    public interface IWorkItemService
    {
        Task<WorkItem> CreateAsync(WorkItemCreateDto dto, int createdById);
        Task<WorkItem> UpdateAsync(int id, WorkItemUpdateDto dto);
        Task DeleteAsync(int id);
        Task<WorkItem?> GetByIdAsync(int id, bool includeHierarchy = false);
        Task<IEnumerable<WorkItem>> GetByBoardAsync(int boardId, bool includeHierarchy = false);
        Task<IEnumerable<WorkItem>> GetHierarchyRootsAsync(int boardId);
        Task MoveToParentAsync(int itemId, int? newParentId);
        Task AssignToSprintAsync(int sprintId, AssignSprintDto dto, int userId);
        Task<IEnumerable<WorkItem>> GetPathToRootAsync(int itemId);
        Task<IEnumerable<WorkItem>> GetBacklogItemsAsync(int projectId);
        Task MoveToBoardAsync(int workItemId, int? boardId, int userId);
        Task ReturnToBacklogAsync(int workItemId, int userId);
        Task ReorderBacklogItemsAsync(int projectId, List<Controllers.ItemOrder> itemOrders, int userId);
    }
}