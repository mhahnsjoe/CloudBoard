namespace CloudBoard.Api.Models
{
    /// <summary>
    /// Represents a work item in the hierarchy (Epic → Feature → PBI → Task/Bug).
    /// Uses single-table design with type discrimination for flexibility.
    /// </summary>
    public class WorkItem
    {
        #region Core Properties
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Status { get; set; } = "To Do";
        public string Priority { get; set; } = "Medium";
        public WorkItemType Type { get; set; } = WorkItemType.Task;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        #endregion

        #region Relationships
        // Board relationship
        public int BoardId { get; set; }
        public Board? Board { get; set; }

        // Hierarchy relationships
        public int? ParentId { get; set; }
        public WorkItem? Parent { get; set; }
        public ICollection<WorkItem> Children { get; set; } = new List<WorkItem>();
        #endregion

        #region Computed Properties
        /// <summary>
        /// Calculates depth in hierarchy (0 for top-level items)
        /// </summary>
        public int Level => CalculateLevel();

        /// <summary>
        /// Indicates if this item can have children based on type
        /// </summary>
        public bool CanHaveChildren => Type.CanHaveChildren();

        /// <summary>
        /// Indicates if this item currently has children
        /// </summary>
        public bool HasChildren => Children?.Any() ?? false;

        /// <summary>
        /// Aggregates estimated hours from entire subtree
        /// </summary>
        public decimal? TotalEstimatedHours => CalculateTotalEstimatedHours();

        /// <summary>
        /// Aggregates actual hours from entire subtree
        /// </summary>
        public decimal? TotalActualHours => CalculateTotalActualHours();

        /// <summary>
        /// Calculates completion percentage based on children
        /// </summary>
        public decimal CompletionPercentage => CalculateCompletionPercentage();
        #endregion

        #region Private Calculation Methods
        private int CalculateLevel()
        {
            var level = 0;
            var current = Parent;
            while (current != null)
            {
                level++;
                current = current.Parent;
            }
            return level;
        }

        private decimal? CalculateTotalEstimatedHours()
        {
            if (!HasChildren)
                return EstimatedHours;

            var childrenHours = Children.Sum(c => c.TotalEstimatedHours ?? 0);
            return (EstimatedHours ?? 0) + childrenHours;
        }

        private decimal? CalculateTotalActualHours()
        {
            if (!HasChildren)
                return ActualHours;

            var childrenHours = Children.Sum(c => c.TotalActualHours ?? 0);
            return (ActualHours ?? 0) + childrenHours;
        }

        private decimal CalculateCompletionPercentage()
        {
            if (!HasChildren)
                return Status == "Done" ? 100 : 0;

            var completedChildren = Children.Count(c => c.Status == "Done");
            return Children.Count == 0 ? 0 : (decimal)completedChildren / Children.Count * 100;
        }
        #endregion
    }

    /// <summary>
    /// Work item type hierarchy: Epic (0) → Feature (1) → PBI (2) → Task (3)
    /// Bug (-1) can exist at any level
    /// </summary>
    public enum WorkItemType
    {
        Task = 0,      // Leaf node - cannot have children
        Bug = 1,       // Flexible - can be standalone or child of Epic/Feature/PBI
        PBI = 2,       // Product Backlog Item - can have Tasks/Bugs
        Feature = 3,   // Can have PBIs
        Epic = 4       // Top-level - can have Features
    }
}