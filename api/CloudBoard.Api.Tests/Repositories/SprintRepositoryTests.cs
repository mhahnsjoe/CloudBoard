using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Repositories;
using FluentAssertions;

namespace CloudBoard.Api.Tests.Repositories;

public class SprintRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetByIdAsync_ExistingSprint_ReturnsSprint()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        await SeedSprintAsync(context, id: 1, boardId: 1, status: SprintStatus.Active);
        var repository = new SprintRepository(context);

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Sprint 1");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new SprintRepository(context);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBoardAsync_ReturnsOnlyBoardSprints_OrderedByStartDateDesc()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        await SeedBoardAsync(context, id: 2, projectId: 1);

        await SeedSprintAsync(context, id: 1, boardId: 1, status: SprintStatus.Completed,
            startDate: DateTime.UtcNow.AddDays(-28));
        await SeedSprintAsync(context, id: 2, boardId: 1, status: SprintStatus.Active,
            startDate: DateTime.UtcNow);
        await SeedSprintAsync(context, id: 3, boardId: 2, status: SprintStatus.Active); // Different board

        var repository = new SprintRepository(context);

        // Act
        var result = await repository.GetByBoardAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.BoardId.Should().Be(1));
        result.First().Id.Should().Be(2); // Most recent first
    }

    [Fact]
    public async Task GetActiveSprintAsync_ReturnsActiveSprint()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        await SeedSprintAsync(context, id: 1, boardId: 1, status: SprintStatus.Completed);
        await SeedSprintAsync(context, id: 2, boardId: 1, status: SprintStatus.Active);
        await SeedSprintAsync(context, id: 3, boardId: 1, status: SprintStatus.Planning);
        var repository = new SprintRepository(context);

        // Act
        var result = await repository.GetActiveSprintAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Status.Should().Be(SprintStatus.Active);
    }

    [Fact]
    public async Task GetActiveSprintAsync_NoActiveSprint_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        await SeedSprintAsync(context, id: 1, boardId: 1, status: SprintStatus.Completed);
        var repository = new SprintRepository(context);

        // Act
        var result = await repository.GetActiveSprintAsync(1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWithWorkItemsAsync_IncludesWorkItems()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        await SeedSprintAsync(context, id: 1, boardId: 1, status: SprintStatus.Active);
        await SeedWorkItemWithSprintAsync(context, id: 1, sprintId: 1);
        await SeedWorkItemWithSprintAsync(context, id: 2, sprintId: 1);
        var repository = new SprintRepository(context);

        // Act
        var result = await repository.GetWithWorkItemsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.WorkItems.Should().HaveCount(2);
    }

    [Fact]
    public async Task HasActiveSprintAsync_WithActiveSprint_ReturnsTrue()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        await SeedSprintAsync(context, id: 1, boardId: 1, status: SprintStatus.Active);
        await SeedSprintAsync(context, id: 2, boardId: 1, status: SprintStatus.Planning);
        var repository = new SprintRepository(context);

        // Act
        var result = await repository.HasActiveSprintAsync(1, excludeSprintId: 2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveSprintAsync_NoOtherActiveSprint_ReturnsFalse()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        await SeedSprintAsync(context, id: 1, boardId: 1, status: SprintStatus.Active);
        var repository = new SprintRepository(context);

        // Act - Excluding the only active sprint
        var result = await repository.HasActiveSprintAsync(1, excludeSprintId: 1);

        // Assert
        result.Should().BeFalse();
    }

    #region Seed Helpers

    private async Task SeedSprintAsync(
        CloudBoardContext context,
        int id,
        int boardId,
        SprintStatus status,
        DateTime? startDate = null)
    {
        context.Sprints.Add(new Sprint
        {
            Id = id,
            Name = $"Sprint {id}",
            BoardId = boardId,
            StartDate = startDate ?? DateTime.UtcNow,
            EndDate = (startDate ?? DateTime.UtcNow).AddDays(14),
            Status = status
        });
        await context.SaveChangesAsync();
    }

    private async Task SeedWorkItemWithSprintAsync(CloudBoardContext context, int id, int sprintId)
    {
        context.WorkItems.Add(new WorkItem
        {
            Id = id,
            Title = $"WorkItem {id}",
            Type = WorkItemType.Task,
            BoardId = 1,
            ProjectId = 1,
            SprintId = sprintId,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
    }

    #endregion
}
