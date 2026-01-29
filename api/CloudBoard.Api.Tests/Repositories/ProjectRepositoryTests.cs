using CloudBoard.Api.Models;
using CloudBoard.Api.Repositories;
using FluentAssertions;

namespace CloudBoard.Api.Tests.Repositories;

public class ProjectRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task GetByIdAsync_ExistingProject_ReturnsProject()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        var repository = new ProjectRepository(context);

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Project 1");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new ProjectRepository(context);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOwnerWithBoardsAsync_ReturnsOnlyOwnedProjects()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1, ownerId: 1);
        await SeedProjectAsync(context, id: 2, ownerId: 2);
        var repository = new ProjectRepository(context);

        // Act
        var result = await repository.GetByOwnerWithBoardsAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetWithBoardsAsync_IncludesBoards()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1);
        await SeedBoardAsync(context, id: 1, projectId: 1);
        var repository = new ProjectRepository(context);

        // Act
        var result = await repository.GetWithBoardsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Boards.Should().HaveCount(1);
    }

    [Fact]
    public async Task IsOwnerAsync_WhenOwner_ReturnsTrue()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1, ownerId: 1);
        var repository = new ProjectRepository(context);

        // Act
        var result = await repository.IsOwnerAsync(projectId: 1, userId: 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsOwnerAsync_WhenNotOwner_ReturnsFalse()
    {
        // Arrange
        using var context = CreateContext();
        await SeedProjectAsync(context, id: 1, ownerId: 1);
        var repository = new ProjectRepository(context);

        // Act
        var result = await repository.IsOwnerAsync(projectId: 1, userId: 999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Add_ThenSaveChanges_PersistsProject()
    {
        // Arrange
        using var context = CreateContext();
        await SeedUserAsync(context, 1);
        var repository = new ProjectRepository(context);
        var project = new Project
        {
            Name = "New Project",
            Description = "New project description",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        repository.Add(project);
        await repository.SaveChangesAsync();

        // Assert
        var saved = await repository.GetByIdAsync(project.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("New Project");
    }

    [Fact]
    public async Task Remove_ThenSaveChanges_DeletesProject()
    {
        // Arrange
        using var context = CreateContext();
        var project = await SeedProjectAsync(context, id: 1);
        var repository = new ProjectRepository(context);

        // Act
        repository.Remove(project);
        await repository.SaveChangesAsync();

        // Assert
        var result = await repository.GetByIdAsync(1);
        result.Should().BeNull();
    }
}
