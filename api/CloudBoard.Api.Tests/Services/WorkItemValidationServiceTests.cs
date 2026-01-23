using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Services;
using CloudBoard.Api.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Tests.Services;

/// <summary>
/// Tests for WorkItemValidationService business rules.
/// Focus: Hierarchy validation, cycle detection, type change validation.
/// </summary>
public class WorkItemValidationServiceTests : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;

    public WorkItemValidationServiceTests(DbContextFixture fixture)
    {
        _fixture = fixture;
    }

    #region ValidateParentChild Tests

    [Theory]
    [InlineData(WorkItemType.Epic, WorkItemType.Feature, true)]
    [InlineData(WorkItemType.Epic, WorkItemType.Bug, true)]
    [InlineData(WorkItemType.Epic, WorkItemType.PBI, false)]
    [InlineData(WorkItemType.Epic, WorkItemType.Task, false)]
    [InlineData(WorkItemType.Feature, WorkItemType.PBI, true)]
    [InlineData(WorkItemType.Feature, WorkItemType.Bug, true)]
    [InlineData(WorkItemType.Feature, WorkItemType.Task, false)]
    [InlineData(WorkItemType.PBI, WorkItemType.Task, true)]
    [InlineData(WorkItemType.PBI, WorkItemType.Bug, true)]
    [InlineData(WorkItemType.Task, WorkItemType.Bug, false)]
    [InlineData(WorkItemType.Task, WorkItemType.Task, false)]
    [InlineData(WorkItemType.Bug, WorkItemType.Task, false)]
    public void ValidateParentChild_EnforcesHierarchyRules(
        WorkItemType parentType,
        WorkItemType childType,
        bool expectedValid)
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var service = new WorkItemValidationService(context);

        // Act
        var result = service.ValidateParentChild(parentType, childType);

        // Assert
        result.IsValid.Should().Be(expectedValid,
            $"{parentType} -> {childType} should be {(expectedValid ? "valid" : "invalid")}");
    }

    [Fact]
    public void ValidateParentChild_InvalidRelationship_ReturnsDescriptiveError()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var service = new WorkItemValidationService(context);

        // Act
        var result = service.ValidateParentChild(WorkItemType.Task, WorkItemType.PBI);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Task");
        result.ErrorMessage.Should().Contain("Product Backlog Item");
        result.ErrorMessage.Should().Contain("cannot have");
    }

    #endregion

    #region ValidateNoCycle Tests

    [Fact]
    public async Task ValidateNoCycle_NullParent_ReturnsSuccess()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var service = new WorkItemValidationService(context);
        await SeedWorkItem(context, 1, "Item 1", null);

        // Act
        var result = await service.ValidateNoCycle(1, null);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateNoCycle_SelfReference_ReturnsFailed()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var service = new WorkItemValidationService(context);
        await SeedWorkItem(context, 1, "Item 1", null);

        // Act
        var result = await service.ValidateNoCycle(1, 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("own parent");
    }

    [Fact]
    public async Task ValidateNoCycle_DirectCycle_ReturnsFailed()
    {
        // Arrange
        // Item 1 -> Item 2 (Item 2's parent is Item 1)
        // Now try to make Item 1's parent = Item 2 (would create cycle)
        using var context = _fixture.CreateContext();
        await SeedWorkItem(context, 1, "Item 1", null);
        await SeedWorkItem(context, 2, "Item 2", 1);

        var service = new WorkItemValidationService(context);

        // Act - Try to set Item 1's parent to Item 2 (its child)
        var result = await service.ValidateNoCycle(1, 2);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("circular");
    }

    [Fact]
    public async Task ValidateNoCycle_DeepCycle_ReturnsFailed()
    {
        // Arrange: A -> B -> C -> D
        // Try to make A's parent = D (creates D -> A -> B -> C -> D cycle)
        using var context = _fixture.CreateContext();
        await SeedWorkItem(context, 1, "A", null);
        await SeedWorkItem(context, 2, "B", 1);
        await SeedWorkItem(context, 3, "C", 2);
        await SeedWorkItem(context, 4, "D", 3);

        var service = new WorkItemValidationService(context);

        // Act
        var result = await service.ValidateNoCycle(1, 4);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("circular");
    }

    [Fact]
    public async Task ValidateNoCycle_ValidNewParent_ReturnsSuccess()
    {
        // Arrange: A, B (siblings, both orphans)
        using var context = _fixture.CreateContext();
        await SeedWorkItem(context, 1, "A", null);
        await SeedWorkItem(context, 2, "B", null);

        var service = new WorkItemValidationService(context);

        // Act - Make B's parent = A (valid)
        var result = await service.ValidateNoCycle(2, 1);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateNoCycle_NonExistentItem_ReturnsFailed()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var service = new WorkItemValidationService(context);

        // Act
        var result = await service.ValidateNoCycle(999, 1);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    #endregion

    #region ValidateDelete Tests

    [Fact]
    public void ValidateDelete_ItemWithChildren_ReturnsFailed()
    {
        // Arrange
        var item = new WorkItem
        {
            Id = 1,
            Title = "Parent",
            Type = WorkItemType.Epic,
            Children = new List<WorkItem>
            {
                new WorkItem { Id = 2, Title = "Child 1" },
                new WorkItem { Id = 3, Title = "Child 2" }
            }
        };

        using var context = _fixture.CreateContext();
        var service = new WorkItemValidationService(context);

        // Act
        var result = service.ValidateDelete(item);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("2 child items");
    }

    [Fact]
    public void ValidateDelete_ItemWithoutChildren_ReturnsSuccess()
    {
        // Arrange
        var item = new WorkItem
        {
            Id = 1,
            Title = "Leaf Item",
            Type = WorkItemType.Task,
            Children = new List<WorkItem>()
        };

        using var context = _fixture.CreateContext();
        var service = new WorkItemValidationService(context);

        // Act
        var result = service.ValidateDelete(item);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ValidateTypeChange Tests

    [Fact]
    public void ValidateTypeChange_WithIncompatibleChildren_ReturnsFailed()
    {
        // Arrange: Feature with PBI children - cannot change to Task
        var feature = new WorkItem
        {
            Id = 1,
            Title = "Feature",
            Type = WorkItemType.Feature,
            Children = new List<WorkItem>
            {
                new WorkItem { Id = 2, Title = "PBI 1", Type = WorkItemType.PBI }
            }
        };

        using var context = _fixture.CreateContext();
        var service = new WorkItemValidationService(context);

        // Act
        var result = service.ValidateTypeChange(feature, WorkItemType.Task);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("incompatible children");
    }

    [Fact]
    public void ValidateTypeChange_WithCompatibleChildren_ReturnsSuccessWithWarning()
    {
        // Arrange: Feature with Bug children - can change to Epic (Epic allows Bug)
        var feature = new WorkItem
        {
            Id = 1,
            Title = "Feature",
            Type = WorkItemType.Feature,
            Children = new List<WorkItem>
            {
                new WorkItem { Id = 2, Title = "Bug 1", Type = WorkItemType.Bug }
            }
        };

        using var context = _fixture.CreateContext();
        var service = new WorkItemValidationService(context);

        // Act - Change Feature to Epic (Epic can have Bug children)
        var result = service.ValidateTypeChange(feature, WorkItemType.Epic);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().Contain(w => w.Contains("1 child"));
    }

    [Fact]
    public void ValidateTypeChange_WithInvalidParentRelationship_ReturnsFailed()
    {
        // Arrange: Task under PBI - cannot change Task to Epic (PBI can't have Epic children)
        var task = new WorkItem
        {
            Id = 2,
            Title = "Task",
            Type = WorkItemType.Task,
            ParentId = 1,
            Parent = new WorkItem
            {
                Id = 1,
                Title = "PBI",
                Type = WorkItemType.PBI
            },
            Children = new List<WorkItem>()
        };

        using var context = _fixture.CreateContext();
        var service = new WorkItemValidationService(context);

        // Act
        var result = service.ValidateTypeChange(task, WorkItemType.Epic);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot have");
    }

    #endregion

    #region Helper Methods

    private async Task SeedWorkItem(
        CloudBoardContext context,
        int id,
        string title,
        int? parentId,
        WorkItemType type = WorkItemType.Task)
    {
        // Ensure user exists for CreatedById
        if (!await context.Users.AnyAsync(u => u.Id == 1))
        {
            context.Users.Add(new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@test.com",
                Name = "Test"
            });
            await context.SaveChangesAsync();
        }

        // Ensure project exists
        if (!await context.Projects.AnyAsync(p => p.Id == 1))
        {
            context.Projects.Add(new Project
            {
                Id = 1,
                Name = "Test Project",
                Description = "Test project description",
                OwnerId = 1
            });
            await context.SaveChangesAsync();
        }

        context.WorkItems.Add(new WorkItem
        {
            Id = id,
            Title = title,
            ParentId = parentId,
            Type = type,
            ProjectId = 1,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    #endregion
}
