namespace CloudBoard.Api.Services
{
    using CloudBoard.Api.Models;
    /// <summary>
    /// Defines validation rules for work item hierarchy.
    /// Interface allows for testing and future alternative implementations.
    /// </summary>
    public interface IWorkItemValidationService
    {
        /// <summary>
        /// Validates if a parent-child relationship is allowed
        /// </summary>
        ValidationResult ValidateParentChild(WorkItemType parentType, WorkItemType childType);

        /// <summary>
        /// Validates if moving an item to a new parent would create a cycle
        /// </summary>
        Task<ValidationResult> ValidateNoCycle(int itemId, int? newParentId);

        /// <summary>
        /// Validates if a work item can be deleted
        /// </summary>
        ValidationResult ValidateDelete(WorkItem item);

        /// <summary>
        /// Validates if a type change is allowed
        /// </summary>
        ValidationResult ValidateTypeChange(WorkItem item, WorkItemType newType);
    }

    /// <summary>
    /// Result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Warnings { get; set; } = new();

        public static ValidationResult Success() => new() { IsValid = true };
        
        public static ValidationResult Failure(string errorMessage) => new() 
        { 
            IsValid = false, 
            ErrorMessage = errorMessage 
        };

        public ValidationResult WithWarning(string warning)
        {
            Warnings.Add(warning);
            return this;
        }
    }
}