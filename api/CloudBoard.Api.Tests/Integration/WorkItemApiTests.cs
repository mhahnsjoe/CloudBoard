using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CloudBoard.Api.Tests.Integration;

/// <summary>
/// End-to-end tests for WorkItem API endpoints.
/// Tests complete flows: Create Project → Create Board → CRUD WorkItems.
/// </summary>
public class WorkItemApiTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateWorkItem_ValidInput_ReturnsCreated()
    {
        // Arrange
        var projectId = await CreateTestProjectAsync();
        var boardId = await GetDefaultBoardAsync(projectId);

        // Act
        var response = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "Integration Test Task",
            Type = "Task",
            Status = "To Do",
            Priority = "Medium"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var workItem = await response.Content.ReadFromJsonAsync<WorkItemResponse>();
        workItem.Should().NotBeNull();
        workItem!.Title.Should().Be("Integration Test Task");
        workItem.BoardId.Should().Be(boardId);
    }

    [Fact]
    public async Task CreateWorkItem_WithParent_EstablishesHierarchy()
    {
        // Arrange
        var projectId = await CreateTestProjectAsync();
        var boardId = await GetDefaultBoardAsync(projectId);

        // Create Epic
        var epicResponse = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "Parent Epic",
            Type = "Epic",
            Status = "To Do",
            Priority = "Medium"
        });
        var epic = await epicResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        // Act - Create Feature under Epic
        var featureResponse = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "Child Feature",
            Type = "Feature",
            Status = "To Do",
            Priority = "Medium",
            ParentId = epic!.Id
        });

        // Assert
        featureResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var feature = await featureResponse.Content.ReadFromJsonAsync<WorkItemResponse>();
        feature!.ParentId.Should().Be(epic.Id);
    }

    [Fact]
    public async Task CreateWorkItem_InvalidHierarchy_ReturnsBadRequest()
    {
        // Arrange
        var projectId = await CreateTestProjectAsync();
        var boardId = await GetDefaultBoardAsync(projectId);

        // Create Task (cannot have children)
        var taskResponse = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "Parent Task",
            Type = "Task",
            Status = "To Do",
            Priority = "Medium"
        });
        var task = await taskResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        // Act - Try to create PBI under Task (invalid)
        var pbiResponse = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "Invalid Child",
            Type = "PBI",
            Status = "To Do",
            Priority = "Medium",
            ParentId = task!.Id
        });

        // Assert
        pbiResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateWorkItem_ValidUpdate_ReturnsOk()
    {
        // Arrange
        var projectId = await CreateTestProjectAsync();
        var boardId = await GetDefaultBoardAsync(projectId);

        var createResponse = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "Original Title",
            Type = "Task",
            Status = "To Do",
            Priority = "Medium"
        });
        var workItem = await createResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        // Act
        var updateResponse = await Client.PutAsJsonAsync(
            $"/api/boards/{boardId}/workitems/{workItem!.Id}",
            new
            {
                Title = "Updated Title",
                Type = "Task",
                Status = "In Progress",
                Priority = "High",
                EstimatedHours = 8,
                BoardId = boardId
            });

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update by fetching the item
        var getResponse = await Client.GetAsync($"/api/boards/{boardId}/workitems/{workItem.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();
        updated!.Title.Should().Be("Updated Title");
        updated.Status.Should().Be("In Progress");
        updated.Priority.Should().Be("High");
    }

    [Fact]
    public async Task DeleteWorkItem_ExistingItem_ReturnsNoContent()
    {
        // Arrange
        var projectId = await CreateTestProjectAsync();
        var boardId = await GetDefaultBoardAsync(projectId);

        var createResponse = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "To Delete",
            Type = "Task",
            Status = "To Do",
            Priority = "Medium"
        });
        var workItem = await createResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        // Act
        var deleteResponse = await Client.DeleteAsync(
            $"/api/boards/{boardId}/workitems/{workItem!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var getResponse = await Client.GetAsync(
            $"/api/boards/{boardId}/workitems/{workItem.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWorkItems_ByBoard_ReturnsCorrectItems()
    {
        // Arrange
        var projectId = await CreateTestProjectAsync();
        var boardId = await GetDefaultBoardAsync(projectId);

        // Create multiple items
        for (int i = 0; i < 3; i++)
        {
            await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
            {
                Title = $"Task {i}",
                Type = "Task",
                Status = "To Do",
                Priority = "Medium"
            });
        }

        // Act
        var response = await Client.GetAsync($"/api/boards/{boardId}/workitems");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var workItems = await response.Content.ReadFromJsonAsync<List<WorkItemResponse>>();
        workItems.Should().HaveCount(3);
    }

    [Fact]
    public async Task CompleteHierarchyFlow_EpicToTask_Succeeds()
    {
        // Arrange
        var projectId = await CreateTestProjectAsync();
        var boardId = await GetDefaultBoardAsync(projectId);

        // Act - Create full hierarchy
        var epicResponse = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "Epic",
            Type = "Epic",
            Status = "To Do",
            Priority = "High"
        });
        var epic = await epicResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        var featureResponse = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "Feature",
            Type = "Feature",
            Status = "To Do",
            Priority = "Medium",
            ParentId = epic!.Id
        });
        var feature = await featureResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        var pbiResponse = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "PBI",
            Type = "PBI",
            Status = "To Do",
            Priority = "Medium",
            ParentId = feature!.Id
        });
        var pbi = await pbiResponse.Content.ReadFromJsonAsync<WorkItemResponse>();

        var taskResponse = await Client.PostAsJsonAsync($"/api/boards/{boardId}/workitems", new
        {
            Title = "Task",
            Type = "Task",
            Status = "To Do",
            Priority = "Low",
            ParentId = pbi!.Id
        });

        // Assert
        taskResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify hierarchy via GET with hierarchy
        var hierarchyResponse = await Client.GetAsync(
            $"/api/boards/{boardId}/workitems/hierarchy");
        var roots = await hierarchyResponse.Content.ReadFromJsonAsync<List<WorkItemResponse>>();

        roots.Should().HaveCount(1);
        roots![0].Title.Should().Be("Epic");
    }

    // Response DTO
    private record WorkItemResponse(
        int Id,
        string Title,
        string Type,
        string Status,
        string Priority,
        int BoardId,
        int? ParentId,
        List<WorkItemResponse>? Children);
}
