using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Services;
using CloudBoard.Api.Tests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CloudBoard.Api.Tests.Services;

/// <summary>
/// Tests for WorkItemService CRUD operations and business logic.
/// Uses real in-memory database with mocked validation service for isolation.
/// </summary>
public class WorkItemServiceTests : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _fixture;

    public WorkItemServiceTests(DbContextFixture fixture)
    {
        _fixture = fixture;
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidBoard_CreatesWorkItem()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);

        var mockValidation = new Mock<IWorkItemValidationService>();
        mockValidation.Setup(v => v.ValidateParentChild(It.IsAny<WorkItemType>(), It.IsAny<WorkItemType>()))
            .Returns(ValidationResult.Success());

        var service = new WorkItemService(context, mockValidation.Object);

        var dto = new WorkItemCreateDto
        {
            Title = "New Task",
            Type = WorkItemType.Task,
            BoardId = 1,
            Status = "To Do",
            Priority = "Medium"
        };

        // Act
        var result = await service.CreateAsync(dto, 1);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Task");
        result.BoardId.Should().Be(1);
        result.ProjectId.Should().Be(1); // Should be inferred from board
        result.BacklogOrder.Should().BeNull(); // Not a backlog item
    }

    [Fact]
    public async Task CreateAsync_AsBacklogItem_AssignsBacklogOrder()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);

        var mockValidation = new Mock<IWorkItemValidationService>();
        var service = new WorkItemService(context, mockValidation.Object);

        var dto = new WorkItemCreateDto
        {
            Title = "Backlog Item",
            Type = WorkItemType.PBI,
            BoardId = null, // Backlog item
            ProjectId = 1,
            Status = "To Do",
            Priority = "Medium"
        };

        // Act
        var result = await service.CreateAsync(dto, 1);

        // Assert
        result.BacklogOrder.Should().NotBeNull();
        result.BacklogOrder.Should().Be(0); // First item starts at 0
    }

    [Fact]
    public async Task CreateAsync_SecondBacklogItem_IncrementsBacklogOrder()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);

        // Add existing backlog item
        context.WorkItems.Add(new WorkItem
        {
            Id = 100,
            Title = "Existing Backlog",
            Type = WorkItemType.PBI,
            ProjectId = 1,
            BoardId = null,
            BacklogOrder = 0,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1
        });
        await context.SaveChangesAsync();

        var mockValidation = new Mock<IWorkItemValidationService>();
        var service = new WorkItemService(context, mockValidation.Object);

        var dto = new WorkItemCreateDto
        {
            Title = "New Backlog Item",
            Type = WorkItemType.PBI,
            BoardId = null,
            ProjectId = 1,
            Status = "To Do",
            Priority = "Medium"
        };

        // Act
        var result = await service.CreateAsync(dto, 1);

        // Assert
        result.BacklogOrder.Should().Be(100); // Incremented by 100
    }

    [Fact]
    public async Task CreateAsync_WithInvalidBoard_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);

        var mockValidation = new Mock<IWorkItemValidationService>();
        var service = new WorkItemService(context, mockValidation.Object);

        var dto = new WorkItemCreateDto
        {
            Title = "Test",
            BoardId = 999, // Non-existent
            Status = "To Do",
            Priority = "Medium"
        };

        // Act & Assert
        await FluentActions.Invoking(() => service.CreateAsync(dto, 1))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task CreateAsync_WithInvalidParent_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);

        var mockValidation = new Mock<IWorkItemValidationService>();
        var service = new WorkItemService(context, mockValidation.Object);

        var dto = new WorkItemCreateDto
        {
            Title = "Test",
            BoardId = 1,
            ParentId = 999, // Non-existent parent
            Status = "To Do",
            Priority = "Medium"
        };

        // Act & Assert
        await FluentActions.Invoking(() => service.CreateAsync(dto, 1))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Parent*");
    }

    [Fact]
    public async Task CreateAsync_ParentOnDifferentBoard_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);

        // Add second board and item on that board
        context.Boards.Add(new Board { Id = 2, Name = "Other Board", ProjectId = 1 });
        context.WorkItems.Add(new WorkItem
        {
            Id = 100,
            Title = "Other Board Item",
            Type = WorkItemType.Epic,
            ProjectId = 1,
            BoardId = 2,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1
        });
        await context.SaveChangesAsync();

        var mockValidation = new Mock<IWorkItemValidationService>();
        mockValidation.Setup(v => v.ValidateParentChild(It.IsAny<WorkItemType>(), It.IsAny<WorkItemType>()))
            .Returns(ValidationResult.Success());

        var service = new WorkItemService(context, mockValidation.Object);

        var dto = new WorkItemCreateDto
        {
            Title = "Test",
            Type = WorkItemType.Feature,
            BoardId = 1,
            ParentId = 100, // Parent is on board 2
            Status = "To Do",
            Priority = "Medium"
        };

        // Act & Assert
        await FluentActions.Invoking(() => service.CreateAsync(dto, 1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*same board*");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ValidUpdate_UpdatesAllFields()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);
        await SeedWorkItem(context, 100, "Original", null, WorkItemType.Task, 1);

        var mockValidation = new Mock<IWorkItemValidationService>();
        mockValidation.Setup(v => v.ValidateNoCycle(It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync(ValidationResult.Success());
        mockValidation.Setup(v => v.ValidateTypeChange(It.IsAny<WorkItem>(), It.IsAny<WorkItemType>()))
            .Returns(ValidationResult.Success());

        var service = new WorkItemService(context, mockValidation.Object);

        var dto = new WorkItemUpdateDto
        {
            Title = "Updated Title",
            Status = "In Progress",
            Priority = "High",
            Type = WorkItemType.Task,
            Description = "New description",
            EstimatedHours = 8,
            BoardId = 1
        };

        // Act
        var result = await service.UpdateAsync(100, dto);

        // Assert
        result.Title.Should().Be("Updated Title");
        result.Status.Should().Be("In Progress");
        result.Priority.Should().Be("High");
        result.Description.Should().Be("New description");
        result.EstimatedHours.Should().Be(8);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentItem_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var mockValidation = new Mock<IWorkItemValidationService>();
        var service = new WorkItemService(context, mockValidation.Object);

        var dto = new WorkItemUpdateDto { Title = "Test", BoardId = 1 };

        // Act & Assert
        await FluentActions.Invoking(() => service.UpdateAsync(999, dto))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_TypeChangeWithInvalidParent_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);
        await SeedWorkItem(context, 100, "Task", 50, WorkItemType.Task, 1);
        await SeedWorkItem(context, 50, "PBI", null, WorkItemType.PBI, 1);

        var mockValidation = new Mock<IWorkItemValidationService>();
        mockValidation.Setup(v => v.ValidateNoCycle(It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync(ValidationResult.Success());
        mockValidation.Setup(v => v.ValidateTypeChange(It.IsAny<WorkItem>(), WorkItemType.Epic))
            .Returns(ValidationResult.Failure("PBI cannot have Epic as child"));

        var service = new WorkItemService(context, mockValidation.Object);

        var dto = new WorkItemUpdateDto
        {
            Title = "Test",
            Type = WorkItemType.Epic, // Try to change Task to Epic under PBI
            ParentId = 50,
            BoardId = 1
        };

        // Act & Assert
        await FluentActions.Invoking(() => service.UpdateAsync(100, dto))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ExistingLeafItem_DeletesSuccessfully()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);
        await SeedWorkItem(context, 100, "To Delete", null, WorkItemType.Task, 1);

        var mockValidation = new Mock<IWorkItemValidationService>();
        mockValidation.Setup(v => v.ValidateDelete(It.IsAny<WorkItem>()))
            .Returns(ValidationResult.Success());

        var service = new WorkItemService(context, mockValidation.Object);

        // Act
        await service.DeleteAsync(100);

        // Assert
        var deleted = await context.WorkItems.FindAsync(100);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ItemWithChildren_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);
        await SeedWorkItem(context, 100, "Parent", null, WorkItemType.Epic, 1);
        await SeedWorkItem(context, 101, "Child", 100, WorkItemType.Feature, 1);

        var mockValidation = new Mock<IWorkItemValidationService>();
        mockValidation.Setup(v => v.ValidateDelete(It.IsAny<WorkItem>()))
            .Returns(ValidationResult.Failure("Has children"));

        var service = new WorkItemService(context, mockValidation.Object);

        // Act & Assert
        await FluentActions.Invoking(() => service.DeleteAsync(100))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region GetByBoardAsync Tests

    [Fact]
    public async Task GetByBoardAsync_ReturnsOnlyBoardItems()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);
        await SeedWorkItem(context, 100, "Board 1 Item", null, WorkItemType.Task, 1);

        // Add item to different board
        context.Boards.Add(new Board { Id = 2, Name = "Board 2", ProjectId = 1 });
        await SeedWorkItem(context, 200, "Board 2 Item", null, WorkItemType.Task, 2);

        var mockValidation = new Mock<IWorkItemValidationService>();
        var service = new WorkItemService(context, mockValidation.Object);

        // Act
        var results = await service.GetByBoardAsync(1);

        // Assert
        results.Should().HaveCount(1);
        results.First().Title.Should().Be("Board 1 Item");
    }

    #endregion

    #region ReorderBacklogItemsAsync Tests

    [Fact]
    public async Task ReorderBacklogItemsAsync_ValidReorder_UpdatesOrders()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);

        // Add backlog items
        context.WorkItems.AddRange(
            new WorkItem { Id = 100, Title = "A", ProjectId = 1, BoardId = null, BacklogOrder = 0, Status = "To Do", Priority = "Medium", CreatedById = 1 },
            new WorkItem { Id = 101, Title = "B", ProjectId = 1, BoardId = null, BacklogOrder = 100, Status = "To Do", Priority = "Medium", CreatedById = 1 },
            new WorkItem { Id = 102, Title = "C", ProjectId = 1, BoardId = null, BacklogOrder = 200, Status = "To Do", Priority = "Medium", CreatedById = 1 }
        );
        await context.SaveChangesAsync();

        var mockValidation = new Mock<IWorkItemValidationService>();
        var service = new WorkItemService(context, mockValidation.Object);

        // Reorder: C, A, B
        var newOrders = new List<CloudBoard.Api.Controllers.ItemOrder>
        {
            new() { ItemId = 102, Order = 0 },
            new() { ItemId = 100, Order = 100 },
            new() { ItemId = 101, Order = 200 }
        };

        // Act
        await service.ReorderBacklogItemsAsync(1, newOrders, 1);

        // Assert
        var items = await context.WorkItems
            .Where(w => w.ProjectId == 1 && w.BoardId == null)
            .OrderBy(w => w.BacklogOrder)
            .ToListAsync();

        items[0].Title.Should().Be("C");
        items[1].Title.Should().Be("A");
        items[2].Title.Should().Be("B");
    }

    [Fact]
    public async Task ReorderBacklogItemsAsync_NonExistentProject_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var mockValidation = new Mock<IWorkItemValidationService>();
        var service = new WorkItemService(context, mockValidation.Object);

        // Act & Assert
        await FluentActions.Invoking(() =>
            service.ReorderBacklogItemsAsync(999, new List<CloudBoard.Api.Controllers.ItemOrder>(), 1))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ReorderBacklogItemsAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        await SeedBasicData(context);

        var mockValidation = new Mock<IWorkItemValidationService>();
        var service = new WorkItemService(context, mockValidation.Object);

        // Act & Assert - User 999 doesn't own project 1
        await FluentActions.Invoking(() =>
            service.ReorderBacklogItemsAsync(1, new List<CloudBoard.Api.Controllers.ItemOrder>(), 999))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region Helper Methods

    private async Task SeedBasicData(CloudBoardContext context)
    {
        if (!await context.Users.AnyAsync(u => u.Id == 1))
        {
            context.Users.Add(new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@test.com",
                Name = "Test User"
            });
            await context.SaveChangesAsync();
        }

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

        if (!await context.Boards.AnyAsync(b => b.Id == 1))
        {
            context.Boards.Add(new Board
            {
                Id = 1,
                Name = "Test Board",
                ProjectId = 1,
                Type = BoardType.Kanban
            });
            await context.SaveChangesAsync();
        }
    }

    private async Task SeedWorkItem(
        CloudBoardContext context,
        int id,
        string title,
        int? parentId,
        WorkItemType type,
        int boardId)
    {
        context.WorkItems.Add(new WorkItem
        {
            Id = id,
            Title = title,
            ParentId = parentId,
            Type = type,
            BoardId = boardId,
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
