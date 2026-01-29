using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Repositories;
using FluentAssertions;

namespace CloudBoard.Api.Tests.Repositories;

public class WorkItemRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetByIdAsync_ExistingWorkItem_ReturnsWorkItem()
    {
        // Arrange
        using var context = CreateContext();
        await SeedFullHierarchyAsync(context);
        var repository = new WorkItemRepository(context);

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("WorkItem 1");
    }

    [Fact]
    public async Task GetWithHierarchyAsync_IncludesParentAndChildren()
    {
        // Arrange
        using var context = CreateContext();
        await SeedFullHierarchyAsync(context);
        // WorkItem 2 has parent (1) and child (3)
        var repository = new WorkItemRepository(context);

        // Act
        var result = await repository.GetWithHierarchyAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.Parent.Should().NotBeNull();
        result.Parent!.Id.Should().Be(1);
        result.Children.Should().HaveCount(1);
        result.Children.First().Id.Should().Be(3);
    }

    [Fact]
    public async Task GetByBoardAsync_ReturnsOnlyBoardWorkItems()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        await SeedBoardAsync(context, id: 2, projectId: 1);
        await SeedWorkItemOnBoardAsync(context, id: 1, boardId: 1, projectId: 1);
        await SeedWorkItemOnBoardAsync(context, id: 2, boardId: 1, projectId: 1);
        await SeedWorkItemOnBoardAsync(context, id: 3, boardId: 2, projectId: 1);
        var repository = new WorkItemRepository(context);

        // Act
        var result = await repository.GetByBoardAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(w => w.BoardId.Should().Be(1));
    }

    [Fact]
    public async Task GetBacklogAsync_ReturnsOnlyBacklogItems_OrderedByBacklogOrder()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);

        // Board items (should not appear)
        await SeedWorkItemOnBoardAsync(context, id: 1, boardId: 1, projectId: 1);

        // Backlog items (should appear, ordered)
        await SeedBacklogItemAsync(context, id: 2, projectId: 1, backlogOrder: 200);
        await SeedBacklogItemAsync(context, id: 3, projectId: 1, backlogOrder: 100);
        await SeedBacklogItemAsync(context, id: 4, projectId: 1, backlogOrder: 300);

        var repository = new WorkItemRepository(context);

        // Act
        var result = await repository.GetBacklogAsync(1);

        // Assert
        result.Should().HaveCount(3);
        result.Select(w => w.Id).Should().ContainInOrder(3, 2, 4); // Ordered by BacklogOrder
    }

    [Fact]
    public async Task GetMaxBacklogOrderAsync_ReturnsMaxOrder()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBacklogItemAsync(context, id: 1, projectId: 1, backlogOrder: 100);
        await SeedBacklogItemAsync(context, id: 2, projectId: 1, backlogOrder: 500);
        await SeedBacklogItemAsync(context, id: 3, projectId: 1, backlogOrder: 300);
        var repository = new WorkItemRepository(context);

        // Act
        var result = await repository.GetMaxBacklogOrderAsync(1, null);

        // Assert
        result.Should().Be(500);
    }

    [Fact]
    public async Task GetMaxBacklogOrderAsync_NoBacklogItems_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        var repository = new WorkItemRepository(context);

        // Act
        var result = await repository.GetMaxBacklogOrderAsync(1, null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRootsByBoardAsync_ReturnsOnlyRootItems()
    {
        // Arrange
        using var context = CreateContext();
        await SeedFullHierarchyAsync(context); // Creates parent-child relationships
        var repository = new WorkItemRepository(context);

        // Act
        var result = await repository.GetRootsByBoardAsync(1);

        // Assert
        result.Should().HaveCount(1); // Only the root item (id: 1)
        result.First().ParentId.Should().BeNull();
    }

    [Fact]
    public async Task GetBySprintAsync_ReturnsSprintWorkItems()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        await SeedSprintAsync(context, id: 1, boardId: 1);
        await SeedWorkItemWithSprintAsync(context, id: 1, boardId: 1, projectId: 1, sprintId: 1);
        await SeedWorkItemWithSprintAsync(context, id: 2, boardId: 1, projectId: 1, sprintId: 1);
        await SeedWorkItemOnBoardAsync(context, id: 3, boardId: 1, projectId: 1); // No sprint
        var repository = new WorkItemRepository(context);

        // Act
        var result = await repository.GetBySprintAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(w => w.SprintId.Should().Be(1));
    }

    #region Seed Helpers

    private async Task SeedWorkItemOnBoardAsync(CloudBoardContext context, int id, int boardId, int projectId)
    {
        context.WorkItems.Add(new WorkItem
        {
            Id = id,
            Title = $"WorkItem {id}",
            Type = WorkItemType.Task,
            BoardId = boardId,
            ProjectId = projectId,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    private async Task SeedBacklogItemAsync(CloudBoardContext context, int id, int projectId, int backlogOrder)
    {
        context.WorkItems.Add(new WorkItem
        {
            Id = id,
            Title = $"Backlog {id}",
            Type = WorkItemType.PBI,
            BoardId = null, // Backlog = no board
            ProjectId = projectId,
            BacklogOrder = backlogOrder,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    private async Task SeedSprintAsync(CloudBoardContext context, int id, int boardId)
    {
        context.Sprints.Add(new Sprint
        {
            Id = id,
            Name = $"Sprint {id}",
            BoardId = boardId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            Status = SprintStatus.Active
        });
        await context.SaveChangesAsync();
    }

    private async Task SeedWorkItemWithSprintAsync(CloudBoardContext context, int id, int boardId, int projectId, int sprintId)
    {
        context.WorkItems.Add(new WorkItem
        {
            Id = id,
            Title = $"WorkItem {id}",
            Type = WorkItemType.Task,
            BoardId = boardId,
            ProjectId = projectId,
            SprintId = sprintId,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    private async Task SeedFullHierarchyAsync(CloudBoardContext context)
    {
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);

        // Epic (root) -> Feature -> Task
        context.WorkItems.Add(new WorkItem
        {
            Id = 1,
            Title = "WorkItem 1",
            Type = WorkItemType.Epic,
            BoardId = 1,
            ProjectId = 1,
            ParentId = null,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        });

        context.WorkItems.Add(new WorkItem
        {
            Id = 2,
            Title = "WorkItem 2",
            Type = WorkItemType.Feature,
            BoardId = 1,
            ProjectId = 1,
            ParentId = 1,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        });

        context.WorkItems.Add(new WorkItem
        {
            Id = 3,
            Title = "WorkItem 3",
            Type = WorkItemType.Task,
            BoardId = 1,
            ProjectId = 1,
            ParentId = 2,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    #endregion
}
