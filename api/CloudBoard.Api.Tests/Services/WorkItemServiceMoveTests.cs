using CloudBoard.Api.Models;
using CloudBoard.Api.Repositories;
using CloudBoard.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CloudBoard.Api.Tests.Services;

public class WorkItemServiceMoveTests
{
    private readonly Mock<IWorkItemRepository> _workItemRepoMock;
    private readonly Mock<IBoardRepository> _boardRepoMock;
    private readonly Mock<IProjectRepository> _projectRepoMock;
    private readonly Mock<ISprintRepository> _sprintRepoMock;
    private readonly Mock<IWorkItemHistoryRepository> _historyRepoMock;
    private readonly Mock<IWorkItemValidationService> _validationMock;
    private readonly Mock<ILogger<WorkItemService>> _loggerMock;
    private readonly WorkItemService _service;

    public WorkItemServiceMoveTests()
    {
        _workItemRepoMock = new Mock<IWorkItemRepository>();
        _boardRepoMock = new Mock<IBoardRepository>();
        _projectRepoMock = new Mock<IProjectRepository>();
        _sprintRepoMock = new Mock<ISprintRepository>();
        _historyRepoMock = new Mock<IWorkItemHistoryRepository>();
        _validationMock = new Mock<IWorkItemValidationService>();
        _loggerMock = new Mock<ILogger<WorkItemService>>();

        _service = new WorkItemService(
            _workItemRepoMock.Object,
            _boardRepoMock.Object,
            _projectRepoMock.Object,
            _sprintRepoMock.Object,
            _historyRepoMock.Object,
            _validationMock.Object,
            _loggerMock.Object);
    }

    #region MoveToBoardAsync Tests

    [Fact]
    public async Task MoveToBoardAsync_PBI_MovesWithChildren()
    {
        // Arrange
        var task1 = new WorkItem { Id = 2, Title = "Task 1", Type = WorkItemType.Task, ProjectId = 1, Children = new List<WorkItem>() };
        var task2 = new WorkItem { Id = 3, Title = "Task 2", Type = WorkItemType.Task, ProjectId = 1, Children = new List<WorkItem>() };
        var pbi = new WorkItem
        {
            Id = 1,
            Title = "PBI",
            Type = WorkItemType.PBI,
            ProjectId = 1,
            Children = new List<WorkItem> { task1, task2 }
        };

        var project = new Project { Id = 1, OwnerId = 1 };
        var board = new Board { Id = 10, ProjectId = 1 };

        _workItemRepoMock.Setup(r => r.GetWithHierarchyAsync(1, default)).ReturnsAsync(pbi);
        _projectRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _boardRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(board);

        // Act
        await _service.MoveToBoardAsync(1, 10, null, 1);

        // Assert
        pbi.BoardId.Should().Be(10);
        task1.BoardId.Should().Be(10);
        task2.BoardId.Should().Be(10);
        _workItemRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task MoveToBoardAsync_Epic_ThrowsInvalidOperation()
    {
        // Arrange
        var epic = new WorkItem
        {
            Id = 1,
            Title = "Epic",
            Type = WorkItemType.Epic,
            ProjectId = 1,
            Children = new List<WorkItem>()
        };

        var project = new Project { Id = 1, OwnerId = 1 };

        _workItemRepoMock.Setup(r => r.GetWithHierarchyAsync(1, default)).ReturnsAsync(epic);
        _projectRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);

        // Act & Assert
        var act = () => _service.MoveToBoardAsync(1, 10, null, 1);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only PBI and Bug*");
    }

    [Fact]
    public async Task MoveToBoardAsync_Feature_ThrowsInvalidOperation()
    {
        // Arrange
        var feature = new WorkItem
        {
            Id = 1,
            Title = "Feature",
            Type = WorkItemType.Feature,
            ProjectId = 1,
            Children = new List<WorkItem>()
        };

        var project = new Project { Id = 1, OwnerId = 1 };

        _workItemRepoMock.Setup(r => r.GetWithHierarchyAsync(1, default)).ReturnsAsync(feature);
        _projectRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);

        // Act & Assert
        var act = () => _service.MoveToBoardAsync(1, 10, null, 1);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only PBI and Bug*");
    }

    [Fact]
    public async Task MoveToBoardAsync_Bug_Succeeds()
    {
        // Arrange
        var bug = new WorkItem
        {
            Id = 1,
            Title = "Bug",
            Type = WorkItemType.Bug,
            ProjectId = 1,
            Children = new List<WorkItem>()
        };

        var project = new Project { Id = 1, OwnerId = 1 };
        var board = new Board { Id = 10, ProjectId = 1 };

        _workItemRepoMock.Setup(r => r.GetWithHierarchyAsync(1, default)).ReturnsAsync(bug);
        _projectRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _boardRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(board);

        // Act
        await _service.MoveToBoardAsync(1, 10, null, 1);

        // Assert
        bug.BoardId.Should().Be(10);
    }

    [Fact]
    public async Task MoveToBoardAsync_PreservesParentLink()
    {
        // Arrange
        var feature = new WorkItem { Id = 5, Title = "Feature", Type = WorkItemType.Feature };
        var pbi = new WorkItem
        {
            Id = 1,
            Title = "PBI",
            Type = WorkItemType.PBI,
            ProjectId = 1,
            ParentId = 5,
            Parent = feature,
            Children = new List<WorkItem>()
        };

        var project = new Project { Id = 1, OwnerId = 1 };
        var board = new Board { Id = 10, ProjectId = 1 };

        _workItemRepoMock.Setup(r => r.GetWithHierarchyAsync(1, default)).ReturnsAsync(pbi);
        _projectRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _boardRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(board);

        // Act
        await _service.MoveToBoardAsync(1, 10, null, 1);

        // Assert
        pbi.ParentId.Should().Be(5); // Parent link preserved!
        pbi.BoardId.Should().Be(10);
    }

    [Fact]
    public async Task MoveToBoardAsync_NestedTasks_AllMoved()
    {
        // Arrange: PBI with Task that has child Bug (edge case, but should adhere to recursion if we support deep hierarchy)
        // Wait, tasks shouldn't have children usually, but let's test the recursion anyway.
        
        var childRecurse = new WorkItem 
        { 
            Id = 4, 
            Title = "Child Recursion", 
            Type = WorkItemType.Task, 
            ProjectId = 1,
            Children = new List<WorkItem>()
        };
        var task = new WorkItem 
        { 
            Id = 3, 
            Title = "Task", 
            Type = WorkItemType.Task, 
            ProjectId = 1,
            Children = new List<WorkItem> { childRecurse } 
        };
        var pbi = new WorkItem
        {
            Id = 1,
            Title = "PBI",
            Type = WorkItemType.PBI,
            ProjectId = 1,
            Children = new List<WorkItem> { task }
        };

        var project = new Project { Id = 1, OwnerId = 1 };
        var board = new Board { Id = 10, ProjectId = 1 };

        _workItemRepoMock.Setup(r => r.GetWithHierarchyAsync(1, default)).ReturnsAsync(pbi);
        _projectRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _boardRepoMock.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(board);

        // Act
        await _service.MoveToBoardAsync(1, 10, null, 1);

        // Assert
        pbi.BoardId.Should().Be(10);
        task.BoardId.Should().Be(10);
        childRecurse.BoardId.Should().Be(10);
    }

    #endregion

    #region ReturnToBacklogAsync Tests

    [Fact]
    public async Task ReturnToBacklogAsync_MovesItemAndChildren()
    {
        // Arrange
        var task = new WorkItem 
        { 
            Id = 2, 
            Title = "Task", 
            Type = WorkItemType.Task, 
            ProjectId = 1, 
            BoardId = 10,
            SprintId = 5,
            Children = new List<WorkItem>()
        };
        var pbi = new WorkItem
        {
            Id = 1,
            Title = "PBI",
            Type = WorkItemType.PBI,
            ProjectId = 1,
            BoardId = 10,
            SprintId = 5,
            Children = new List<WorkItem> { task }
        };

        var project = new Project { Id = 1, OwnerId = 1 };

        _workItemRepoMock.Setup(r => r.GetWithHierarchyAsync(1, default)).ReturnsAsync(pbi);
        _projectRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _workItemRepoMock.Setup(r => r.GetMaxBacklogOrderAsync(1, It.IsAny<int?>(), default))
            .ReturnsAsync(0);

        // Act
        await _service.ReturnToBacklogAsync(1, 1);

        // Assert
        pbi.BoardId.Should().BeNull();
        pbi.SprintId.Should().BeNull();
        pbi.BacklogOrder.Should().NotBeNull();
        task.BoardId.Should().BeNull();
        task.SprintId.Should().BeNull();
    }

    #endregion
}
