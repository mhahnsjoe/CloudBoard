using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Tests.Repositories;

/// <summary>
/// Base class for repository tests providing common setup and seed methods.
/// </summary>
public abstract class RepositoryTestBase : IDisposable
{
    protected CloudBoardContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CloudBoardContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new CloudBoardContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    protected async Task SeedUserAsync(CloudBoardContext context, int id = 1)
    {
        if (!await context.Users.AnyAsync(u => u.Id == id))
        {
            context.Users.Add(new User
            {
                Id = id,
                UserName = $"user{id}",
                Email = $"user{id}@test.com",
                Name = $"User {id}"
            });
            await context.SaveChangesAsync();
        }
    }

    protected async Task<Project> SeedProjectAsync(CloudBoardContext context, int id = 1, int ownerId = 1)
    {
        await SeedUserAsync(context, ownerId);

        var project = new Project
        {
            Id = id,
            Name = $"Project {id}",
            Description = "Test project",
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();
        return project;
    }

    protected async Task<Board> SeedBoardAsync(CloudBoardContext context, int id = 1, int projectId = 1)
    {
        var board = new Board
        {
            Id = id,
            Name = $"Board {id}",
            ProjectId = projectId,
            Type = BoardType.Kanban,
            CreatedAt = DateTime.UtcNow
        };

        context.Boards.Add(board);
        await context.SaveChangesAsync();
        return board;
    }

    protected async Task<WorkItem> SeedWorkItemAsync(CloudBoardContext context, int id = 1, int projectId = 1, int? boardId = null)
    {
        var workItem = new WorkItem
        {
            Id = id,
            Title = $"WorkItem {id}",
            ProjectId = projectId,
            BoardId = boardId,
            Type = WorkItemType.Task,
            Status = "To Do",
            Priority = "Medium",
            CreatedAt = DateTime.UtcNow
        };

        context.WorkItems.Add(workItem);
        await context.SaveChangesAsync();
        return workItem;
    }

    public void Dispose()
    {
        // In-memory database auto-disposed
        GC.SuppressFinalize(this);
    }
}
