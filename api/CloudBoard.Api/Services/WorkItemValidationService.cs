using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Services
{
    /// <summary>
    /// Implements business rules for work item hierarchy.
    /// </summary>
    public class WorkItemValidationService : IWorkItemValidationService
    {
        private readonly CloudBoardContext _context;

        public WorkItemValidationService(CloudBoardContext context)
        {
            _context = context;
        }

        public ValidationResult ValidateParentChild(WorkItemType parentType, WorkItemType childType)
        {
            if (!parentType.CanHaveChildType(childType))
            {
                var allowed = parentType.GetAllowedChildTypes();
                var allowedNames = string.Join(", ", allowed.Select(t => t.GetDisplayName()));
                
                return ValidationResult.Failure(
                    $"A {parentType.GetDisplayName()} cannot have a {childType.GetDisplayName()} as a child. " +
                    $"Allowed children: {allowedNames}");
            }

            return ValidationResult.Success();
        }

        public async Task<ValidationResult> ValidateNoCycle(int itemId, int? newParentId)
        {
            if (!newParentId.HasValue)
                return ValidationResult.Success(); // No parent = no cycle possible

            // Check if new parent is the item itself
            if (newParentId == itemId)
                return ValidationResult.Failure("An item cannot be its own parent");

            // Check if new parent is a descendant of this item
            var item = await _context.WorkItems
                .Include(t => t.Children)
                .FirstOrDefaultAsync(t => t.Id == itemId);

            if (item == null)
                return ValidationResult.Failure("Item not found");

            // Walk up the parent chain to detect cycles
            var currentParentId = newParentId;
            var visitedIds = new HashSet<int> { itemId };

            while (currentParentId.HasValue)
            {
                if (visitedIds.Contains(currentParentId.Value))
                {
                    return ValidationResult.Failure(
                        "This would create a circular reference in the hierarchy");
                }

                visitedIds.Add(currentParentId.Value);

                var parent = await _context.WorkItems
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == currentParentId.Value);

                currentParentId = parent?.ParentId;
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateDelete(WorkItem item)
        {
            if (item.HasChildren)
            {
                return ValidationResult.Failure(
                    $"Cannot delete '{item.Title}' because it has {item.Children.Count} child items. " +
                    "Delete or reassign the children first.");
            }

            return ValidationResult.Success();
        }

        public ValidationResult ValidateTypeChange(WorkItem item, WorkItemType newType)
        {
            var result = ValidationResult.Success();

            // Check if new type can be a child of current parent
            if (item.ParentId.HasValue && item.Parent != null)
            {
                var parentValidation = ValidateParentChild(item.Parent.Type, newType);
                if (!parentValidation.IsValid)
                    return parentValidation;
            }

            // Check if current children are valid for new type
            if (item.HasChildren)
            {
                var invalidChildren = item.Children
                    .Where(c => !newType.CanHaveChildType(c.Type))
                    .ToList();

                if (invalidChildren.Any())
                {
                    var childTypes = string.Join(", ", invalidChildren.Select(c => c.Type.GetDisplayName()));
                    return ValidationResult.Failure(
                        $"Cannot change type to {newType.GetDisplayName()} because it has incompatible children: {childTypes}");
                }

                result.WithWarning($"This change will affect {item.Children.Count} child items");
            }

            return result;
        }
    }
}