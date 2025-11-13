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
        Task<WorkItem> CreateAsync(WorkItemCreateDto dto);
        Task<WorkItem> UpdateAsync(int id, WorkItemUpdateDto dto);
        Task DeleteAsync(int id);
        Task<WorkItem?> GetByIdAsync(int id, bool includeHierarchy = false);
        Task<IEnumerable<WorkItem>> GetByBoardAsync(int boardId, bool includeHierarchy = false);
        Task<IEnumerable<WorkItem>> GetHierarchyRootsAsync(int boardId);
        Task MoveToParentAsync(int itemId, int? newParentId);
        Task<IEnumerable<WorkItem>> GetPathToRootAsync(int itemId);
    }
}