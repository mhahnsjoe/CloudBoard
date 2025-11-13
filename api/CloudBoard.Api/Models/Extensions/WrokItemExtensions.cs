namespace CloudBoard.Api.Models.Extensions
{
    /// <summary>
    /// Extension methods for WorkItem instances.
    /// Provides type-specific behavior without inheritance.
    /// </summary>
    public static class WorkItemExtensions
    {
        /// <summary>
        /// Gets all descendant items recursively
        /// </summary>
        public static IEnumerable<WorkItem> GetDescendants(this WorkItem item)
        {
            var descendants = new List<WorkItem>();
            CollectDescendants(item, descendants);
            return descendants;
        }

        private static void CollectDescendants(WorkItem item, List<WorkItem> results)
        {
            if (item.Children == null) return;

            foreach (var child in item.Children)
            {
                results.Add(child);
                CollectDescendants(child, results);
            }
        }

        /// <summary>
        /// Gets all ancestor items up to root
        /// </summary>
        public static IEnumerable<WorkItem> GetAncestors(this WorkItem item)
        {
            var ancestors = new List<WorkItem>();
            var current = item.Parent;

            while (current != null)
            {
                ancestors.Add(current);
                current = current.Parent;
            }

            return ancestors;
        }

        /// <summary>
        /// Gets the root (top-level) item in the hierarchy
        /// </summary>
        public static WorkItem GetRoot(this WorkItem item)
        {
            var current = item;
            while (current.Parent != null)
            {
                current = current.Parent;
            }
            return current;
        }

        /// <summary>
        /// Checks if this item is an ancestor of another item
        /// </summary>
        public static bool IsAncestorOf(this WorkItem ancestor, WorkItem descendant)
        {
            return descendant.GetAncestors().Any(a => a.Id == ancestor.Id);
        }

        /// <summary>
        /// Gets immediate children of a specific type
        /// </summary>
        public static IEnumerable<WorkItem> GetChildrenOfType(this WorkItem item, WorkItemType type)
        {
            return item.Children?.Where(c => c.Type == type) ?? Enumerable.Empty<WorkItem>();
        }

        /// <summary>
        /// Validates if adding a child of given type is allowed
        /// </summary>
        public static bool CanAddChildOfType(this WorkItem parent, WorkItemType childType)
        {
            return parent.Type.CanHaveChildType(childType);
        }

        /// <summary>
        /// Gets a breadcrumb path (Epic > Feature > PBI > Task)
        /// </summary>
        public static string GetBreadcrumbPath(this WorkItem item, string separator = " > ")
        {
            var ancestors = item.GetAncestors().Reverse().ToList();
            ancestors.Add(item);
            return string.Join(separator, ancestors.Select(a => a.Title));
        }
    }
}