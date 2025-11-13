namespace CloudBoard.Api.Models.Extensions
{
    /// <summary>
    /// Type-safe extension methods for WorkItemType enum.
    /// Implements type hierarchy rules without duplicating logic.
    /// </summary>
    public static class WorkItemTypeExtensions
    {
        private static readonly IReadOnlyDictionary<WorkItemType, WorkItemType[]> AllowedChildren = 
            new Dictionary<WorkItemType, WorkItemType[]>
            {
                { WorkItemType.Epic, new[] { WorkItemType.Feature, WorkItemType.Bug } },
                { WorkItemType.Feature, new[] { WorkItemType.PBI, WorkItemType.Bug } },
                { WorkItemType.PBI, new[] { WorkItemType.Task, WorkItemType.Bug } },
                { WorkItemType.Task, Array.Empty<WorkItemType>() },
                { WorkItemType.Bug, Array.Empty<WorkItemType>() }
            };

        private static readonly IReadOnlyDictionary<WorkItemType, int> HierarchyLevels = 
            new Dictionary<WorkItemType, int>
            {
                { WorkItemType.Epic, 0 },
                { WorkItemType.Feature, 1 },
                { WorkItemType.PBI, 2 },
                { WorkItemType.Task, 3 },
                { WorkItemType.Bug, -1 } // Flexible level
            };

        /// <summary>
        /// Determines if this type can have child items
        /// </summary>
        public static bool CanHaveChildren(this WorkItemType type)
        {
            return AllowedChildren.TryGetValue(type, out var children) && children.Length > 0;
        }

        /// <summary>
        /// Gets the allowed child types for this type
        /// </summary>
        public static WorkItemType[] GetAllowedChildTypes(this WorkItemType type)
        {
            return AllowedChildren.TryGetValue(type, out var children) 
                ? children 
                : Array.Empty<WorkItemType>();
        }

        /// <summary>
        /// Validates if a child type can be added to this parent type
        /// </summary>
        public static bool CanHaveChildType(this WorkItemType parent, WorkItemType child)
        {
            return AllowedChildren.TryGetValue(parent, out var allowed) && allowed.Contains(child);
        }

        /// <summary>
        /// Gets the hierarchy level (0 = top, 3 = bottom, -1 = flexible)
        /// </summary>
        public static int GetHierarchyLevel(this WorkItemType type)
        {
            return HierarchyLevels.TryGetValue(type, out var level) ? level : -1;
        }

        /// <summary>
        /// Checks if this is a leaf node type (cannot have children)
        /// </summary>
        public static bool IsLeafType(this WorkItemType type)
        {
            return type == WorkItemType.Task || (type == WorkItemType.Bug && !CanHaveChildren(type));
        }

        /// <summary>
        /// Gets a human-readable display name
        /// </summary>
        public static string GetDisplayName(this WorkItemType type)
        {
            return type switch
            {
                WorkItemType.PBI => "Product Backlog Item",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// Gets an icon identifier for UI rendering
        /// </summary>
        public static string GetIconClass(this WorkItemType type)
        {
            return type switch
            {
                WorkItemType.Epic => "epic-icon",
                WorkItemType.Feature => "feature-icon",
                WorkItemType.PBI => "pbi-icon",
                WorkItemType.Task => "task-icon",
                WorkItemType.Bug => "bug-icon",
                _ => "default-icon"
            };
        }
    }
}