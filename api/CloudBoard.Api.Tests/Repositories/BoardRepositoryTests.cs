using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Repositories;
using FluentAssertions;

namespace CloudBoard.Api.Tests.Repositories;

public class BoardRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetByIdAsync_ExistingBoard_ReturnsBoard()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        var repository = new BoardRepository(context);

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Board 1");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new BoardRepository(context);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWithColumnsAsync_IncludesOrderedColumns()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardWithColumnsAsync(context, boardId: 1, projectId: 1);
        var repository = new BoardRepository(context);

        // Act
        var result = await repository.GetWithColumnsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Columns.Should().HaveCount(3);
        result.Columns.Should().BeInAscendingOrder(c => c.Order);
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsOnlyProjectBoards()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedProjectAsync(context, id: 2, ownerId: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        await SeedBoardAsync(context, id: 2, projectId: 1);
        await SeedBoardAsync(context, id: 3, projectId: 2);
        var repository = new BoardRepository(context);

        // Act
        var result = await repository.GetByProjectAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(b => b.ProjectId.Should().Be(1));
    }

    [Fact]
    public async Task GetProjectIdAsync_ReturnsCorrectProjectId()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 5);
        await SeedBoardAsync(context, id: 1, projectId: 5);
        var repository = new BoardRepository(context);

        // Act
        var result = await repository.GetProjectIdAsync(1);

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task GetProjectIdAsync_NonExistentBoard_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new BoardRepository(context);

        // Act
        var result = await repository.GetProjectIdAsync(999);

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
        await SeedWorkItemOnBoardAsync(context, id: 1, boardId: 1, projectId: 1);
        await SeedWorkItemOnBoardAsync(context, id: 2, boardId: 1, projectId: 1);
        var repository = new BoardRepository(context);

        // Act
        var result = await repository.GetWithWorkItemsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.WorkItems.Should().HaveCount(2);
    }

    #region Additional Seed Helpers

    private async Task SeedBoardWithColumnsAsync(CloudBoardContext context, int boardId, int projectId)
    {
        var board = new Board
        {
            Id = boardId,
            Name = $"Board {boardId}",
            ProjectId = projectId,
            Type = BoardType.Kanban,
            CreatedAt = DateTime.UtcNow,
            Columns = new List<BoardColumn>
            {
                new() { Name = "To Do", Order = 0, Category = "To Do" },
                new() { Name = "In Progress", Order = 1, Category = "In Progress" },
                new() { Name = "Done", Order = 2, Category = "Done" }
            }
        };

        context.Boards.Add(board);
        await context.SaveChangesAsync();
    }

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

    #endregion
}
