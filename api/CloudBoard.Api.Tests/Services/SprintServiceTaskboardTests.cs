using System.Collections.Generic;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using CloudBoard.Api.Repositories;
using CloudBoard.Api.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace CloudBoard.Api.Tests.Services;

public class SprintServiceTaskboardTests
{
    private readonly Mock<ISprintRepository> _sprintRepoMock;
    private readonly Mock<IBoardRepository> _boardRepoMock;
    private readonly Mock<IWorkItemRepository> _workItemRepoMock;
    private readonly Mock<IWorkItemHistoryRepository> _historyRepoMock;
    private readonly SprintService _service;

    public SprintServiceTaskboardTests()
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
    public async Task GetTaskboardAsync_ReturnsPBIsAsRows_WithTasksGrouped()
    {
        // Arrange
        var sprint = CreateTestSprint();
        var workItems = CreateTestWorkItems();

        _sprintRepoMock.Setup(r => r.GetWithFullContextAsync(1, default))
            .ReturnsAsync(sprint);
        _workItemRepoMock.Setup(r => r.GetBySprintAsync(1, default))
            .ReturnsAsync(workItems);

        // Act
        var result = await _service.GetTaskboardAsync(1, 1);

        // Assert
        result.Rows.Should().HaveCount(1);
        result.Rows[0].Tasks.Should().HaveCount(2);
        result.Columns.Should().Contain("To Do");
    }

    [Fact]
    public async Task GetTaskboardAsync_CalculatesProgressCorrectly()
    {
        // Arrange
        var sprint = CreateTestSprint();
        var workItems = new List<WorkItem>
        {
            new() { Id = 1, Title = "PBI", Type = WorkItemType.PBI, Status = "In Progress", SprintId = 1, BoardId = 1, ProjectId = 1 },
            new() { Id = 2, Title = "Task 1", Type = WorkItemType.Task, Status = "Done", ParentId = 1, EstimatedHours = 10, SprintId = 1, BoardId = 1, ProjectId = 1 },
            new() { Id = 3, Title = "Task 2", Type = WorkItemType.Task, Status = "To Do", ParentId = 1, EstimatedHours = 10, SprintId = 1, BoardId = 1, ProjectId = 1 }
        };

        _sprintRepoMock.Setup(r => r.GetWithFullContextAsync(1, default))
            .ReturnsAsync(sprint);
        _workItemRepoMock.Setup(r => r.GetBySprintAsync(1, default))
            .ReturnsAsync(workItems);

        // Act
        var result = await _service.GetTaskboardAsync(1, 1);

        // Assert
        var row = result.Rows[0];
        row.TotalHours.Should().Be(20);
        row.CompletedHours.Should().Be(10);
        row.ProgressPercentage.Should().Be(50);
    }

    [Fact]
    public async Task BulkAssignToSprintAsync_RejectsEpicType()
    {
        // Arrange
        var sprint = CreateTestSprint();
        var epic = new WorkItem { Id = 100, BoardId = 1, Type = WorkItemType.Epic, Title = "Epic", ProjectId = 1 };
        
        _sprintRepoMock.Setup(r => r.GetWithFullContextAsync(1, default))
            .ReturnsAsync(sprint);
        _workItemRepoMock.Setup(r => r.GetByIdAsync(100, default))
            .ReturnsAsync(epic);

        // Act
        var result = await _service.BulkAssignToSprintAsync(1, new List<int> { 100 }, 1);

        // Assert
        result.FailedCount.Should().Be(1);
        result.Errors.Should().Contain(e => e.Contains("Epic") && e.Contains("cannot be added"));
    }

    private static Sprint CreateTestSprint()
    {
        return new Sprint
        {
            Id = 1,
            Name = "Sprint 1",
            BoardId = 1,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            Status = SprintStatus.Active,
            Board = new Board
            {
                Id = 1,
                ProjectId = 1,
                Project = new Project { Id = 1, OwnerId = 1 },
                Columns = new List<BoardColumn>
                {
                    new() { Id = 1, Name = "To Do", Order = 0 },
                    new() { Id = 2, Name = "In Progress", Order = 1 },
                    new() { Id = 3, Name = "Done", Order = 2 }
                }
            }
        };
    }

    private static List<WorkItem> CreateTestWorkItems()
    {
        return new List<WorkItem>
        {
            new() { Id = 1, Title = "PBI 1", Type = WorkItemType.PBI, Status = "In Progress", SprintId = 1, BoardId = 1, ProjectId = 1 },
            new() { Id = 2, Title = "Task 1", Type = WorkItemType.Task, Status = "To Do", ParentId = 1, SprintId = 1, BoardId = 1, ProjectId = 1 },
            new() { Id = 3, Title = "Task 2", Type = WorkItemType.Task, Status = "Done", ParentId = 1, SprintId = 1, BoardId = 1, ProjectId = 1 }
        };
    }
}
