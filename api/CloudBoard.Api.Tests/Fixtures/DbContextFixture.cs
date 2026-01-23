using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Tests.Fixtures;

/// <summary>
/// Provides isolated in-memory database instances for unit tests.
/// Each test gets a fresh database to ensure isolation.
/// </summary>
public class DbContextFixture : IDisposable
{
    public CloudBoardContext CreateContext()
    {
        // Create a unique database for each test to ensure isolation
        var options = new DbContextOptionsBuilder<CloudBoardContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new CloudBoardContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Seeds the database with a standard test hierarchy:
    /// Project -> Board -> Epic -> Feature -> PBI -> Task
    /// </summary>
    public async Task<TestDataSet> SeedStandardHierarchyAsync()
    {
        using var context = CreateContext();

        var user = new User
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@test.com",
            Name = "Test User"
        };
        context.Users.Add(user);

        var project = new Project
        {
            Id = 1,
            Name = "Test Project",
            Description = "Test project description",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.Projects.Add(project);

        var board = new Board
        {
            Id = 1,
            Name = "Test Board",
            ProjectId = 1,
            Type = BoardType.Kanban,
            CreatedAt = DateTime.UtcNow
        };
        context.Boards.Add(board);

        var epic = new WorkItem
        {
            Id = 1,
            Title = "Test Epic",
            Type = WorkItemType.Epic,
            ProjectId = 1,
            BoardId = 1,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.WorkItems.Add(epic);

        var feature = new WorkItem
        {
            Id = 2,
            Title = "Test Feature",
            Type = WorkItemType.Feature,
            ProjectId = 1,
            BoardId = 1,
            ParentId = 1,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.WorkItems.Add(feature);

        var pbi = new WorkItem
        {
            Id = 3,
            Title = "Test PBI",
            Type = WorkItemType.PBI,
            ProjectId = 1,
            BoardId = 1,
            ParentId = 2,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.WorkItems.Add(pbi);

        var task = new WorkItem
        {
            Id = 4,
            Title = "Test Task",
            Type = WorkItemType.Task,
            ProjectId = 1,
            BoardId = 1,
            ParentId = 3,
            Status = "To Do",
            Priority = "Medium",
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.WorkItems.Add(task);

        await context.SaveChangesAsync();

        return new TestDataSet
        {
            UserId = user.Id,
            ProjectId = project.Id,
            BoardId = board.Id,
            EpicId = epic.Id,
            FeatureId = feature.Id,
            PbiId = pbi.Id,
            TaskId = task.Id
        };
    }

    public void Dispose()
    {
        // In-memory database is disposed automatically
    }
}

public class TestDataSet
{
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public int BoardId { get; set; }
    public int EpicId { get; set; }
    public int FeatureId { get; set; }
    public int PbiId { get; set; }
    public int TaskId { get; set; }
}
