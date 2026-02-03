using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Repositories;
using CloudBoard.Api.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace CloudBoard.Api.Tests.Services;

public class SprintServiceBulkOperationsTests
{
    private readonly Mock<ISprintRepository> _sprintRepoMock;
    private readonly Mock<IBoardRepository> _boardRepoMock;
    private readonly Mock<IWorkItemRepository> _workItemRepoMock;
    private readonly Mock<IWorkItemHistoryRepository> _historyRepoMock;
    private readonly SprintService _service;

    public SprintServiceBulkOperationsTests()
    {
        _sprintRepoMock = new Mock<ISprintRepository>();
        _boardRepoMock = new Mock<IBoardRepository>();
        _workItemRepoMock = new Mock<IWorkItemRepository>();
        _historyRepoMock = new Mock<IWorkItemHistoryRepository>();
        _service = new SprintService(
            _sprintRepoMock.Object, 
            _boardRepoMock.Object, 
            _workItemRepoMock.Object,
            _historyRepoMock.Object);
    }

    [Fact]
    public async Task BulkAssignToSprintAsync_ValidItems_AssignsAllItems()
    {
        // Arrange
        var sprint = CreateTestSprint(1, 1, SprintStatus.Active);
        var workItem1 = new WorkItem { Id = 1, BoardId = 1, ProjectId = 1, Title = "Task 1" };
        var workItem2 = new WorkItem { Id = 2, BoardId = 1, ProjectId = 1, Title = "Task 2" };

        _sprintRepoMock.Setup(r => r.GetWithFullContextAsync(1, default))
            .ReturnsAsync(sprint);
        _workItemRepoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(workItem1);
        _workItemRepoMock.Setup(r => r.GetByIdAsync(2, default)).ReturnsAsync(workItem2);
        _historyRepoMock.Setup(r => r.Add(It.IsAny<WorkItemHistory>()));

        // Act
        var result = await _service.BulkAssignToSprintAsync(1, new List<int> { 1, 2 }, 1);

        // Assert
        result.SuccessCount.Should().Be(2);
        result.FailedCount.Should().Be(0);
        workItem1.SprintId.Should().Be(1);
        workItem2.SprintId.Should().Be(1);
    }

    [Fact]
    public async Task BulkAssignToSprintAsync_CompletedSprint_ThrowsInvalidOperation()
    {
        // Arrange
        var sprint = CreateTestSprint(1, 1, SprintStatus.Completed);
        _sprintRepoMock.Setup(r => r.GetWithFullContextAsync(1, default))
            .ReturnsAsync(sprint);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.BulkAssignToSprintAsync(1, new List<int> { 1 }, 1));
    }

    [Fact]
    public async Task BulkUnassignFromSprintAsync_ValidItems_RemovesFromSprint()
    {
        // Arrange
        var workItem = new WorkItem { Id = 1, BoardId = 1, SprintId = 1, Title = "Task 1" };
        var sprint = CreateTestSprint(1, 1, SprintStatus.Active);
        sprint.WorkItems.Add(workItem);

        _sprintRepoMock.Setup(r => r.GetWithFullContextAsync(1, default))
            .ReturnsAsync(sprint);

        // Act
        var result = await _service.BulkUnassignFromSprintAsync(1, new List<int> { 1 }, 1);

        // Assert
        result.SuccessCount.Should().Be(1);
        workItem.SprintId.Should().BeNull();
    }

    [Fact]
    public async Task GetBoardVelocityAsync_CompletedSprints_ReturnsVelocityData()
    {
        // Arrange
        var board = CreateTestBoard(1, 1);
        var sprints = new List<Sprint>
        {
            CreateTestSprintWithWorkItems(1, 1, SprintStatus.Completed, 40, 35),
            CreateTestSprintWithWorkItems(2, 1, SprintStatus.Completed, 50, 45),
        };

        _boardRepoMock.Setup(r => r.GetWithProjectAsync(1, default)).ReturnsAsync(board);
        _sprintRepoMock.Setup(r => r.GetByBoardAsync(1, default)).ReturnsAsync(sprints);

        // Act
        var result = await _service.GetBoardVelocityAsync(1, 1, 6);

        // Assert
        result.SprintVelocities.Should().HaveCount(2);
        result.AverageVelocityHours.Should().Be(40); // (35 + 45) / 2
    }

    private static Sprint CreateTestSprint(int id, int boardId, SprintStatus status)
    {
        return new Sprint
        {
            Id = id,
            Name = $"Sprint {id}",
            BoardId = boardId,
            StartDate = DateTime.UtcNow.AddDays(-14),
            EndDate = DateTime.UtcNow,
            Status = status,
            Board = new Board
            {
                Id = boardId,
                ProjectId = 1,
                Project = new Project { Id = 1, OwnerId = 1 }
            },
            WorkItems = new List<WorkItem>()
        };
    }

    private static Sprint CreateTestSprintWithWorkItems(
        int id, int boardId, SprintStatus status, decimal planned, decimal completed)
    {
        var sprint = CreateTestSprint(id, boardId, status);
        // Add work items to simulate planned/completed hours
        // Note: MapToDto uses sprint.TotalEstimatedHours and sprint.CompletedEstimatedHours 
        // which rely on WorkItems collection
        sprint.WorkItems.Add(new WorkItem 
        { 
            Id = id * 100, 
            EstimatedHours = planned, 
            Status = "Done" // MapToDto counts "Done" for completed hours
        });
        
        // Wait, the Sprint model implementation uses "Done" for completion:
        // public decimal CompletedEstimatedHours => WorkItems?.Where(w => w.Status == "Done").Sum(w => w.EstimatedHours ?? 0) ?? 0;
        
        // However, the test should reflect reality. 
        // In the sprint-execution-guide.md, it says:
        // CreateTestSprintWithWorkItems(1, 1, SprintStatus.Completed, 40, 35)
        // I need to adjust the item creation to match the planned/completed request.
        
        sprint.WorkItems.Clear();
        sprint.WorkItems.Add(new WorkItem { Id = id * 100 + 1, EstimatedHours = completed, Status = "Done" });
        sprint.WorkItems.Add(new WorkItem { Id = id * 100 + 2, EstimatedHours = planned - completed, Status = "To Do" });
        
        return sprint;
    }

    private static Board CreateTestBoard(int id, int ownerId)
    {
        return new Board
        {
            Id = id,
            Name = $"Board {id}",
            ProjectId = 1,
            Project = new Project { Id = 1, OwnerId = ownerId }
        };
    }
}
