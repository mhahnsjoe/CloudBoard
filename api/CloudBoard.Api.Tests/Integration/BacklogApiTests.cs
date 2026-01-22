using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CloudBoard.Api.Tests.Integration;

/// <summary>
/// Integration tests for backlog management operations.
/// </summary>
public class BacklogApiTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateBacklogItem_WithoutBoard_SucceedsWithOrder()
    {
        // Arrange
        var projectId = await CreateTestProjectAsync();

        // Act
        var response = await Client.PostAsJsonAsync($"/api/projects/{projectId}/backlog", new
        {
            Title = "Backlog Item",
            Type = "PBI",
            Status = "To Do",
            Priority = "Medium"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var item = await response.Content.ReadFromJsonAsync<BacklogItemResponse>();
        item!.BoardId.Should().BeNull();
        item.BacklogOrder.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBacklog_ReturnsOrderedItems()
    {
        // Arrange
        var projectId = await CreateTestProjectAsync();

        // Create multiple backlog items
        for (int i = 0; i < 3; i++)
        {
            await Client.PostAsJsonAsync($"/api/projects/{projectId}/backlog", new
            {
                Title = $"Backlog Item {i}",
                Type = "PBI",
                Status = "To Do",
                Priority = "Medium"
            });
        }

        // Act
        var response = await Client.GetAsync($"/api/projects/{projectId}/backlog");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await response.Content.ReadFromJsonAsync<List<BacklogItemResponse>>();
        items.Should().HaveCount(3);

        // Verify ordering
        items![0].BacklogOrder.Should().BeLessThan(items[1].BacklogOrder!.Value);
        items[1].BacklogOrder.Should().BeLessThan(items[2].BacklogOrder!.Value);
    }

    [Fact]
    public async Task MoveToBoard_TransfersItem()
    {
        // Arrange
        var projectId = await CreateTestProjectAsync();
        var boardId = await GetDefaultBoardAsync(projectId);

        var createResponse = await Client.PostAsJsonAsync($"/api/projects/{projectId}/backlog", new
        {
            Title = "Move Me",
            Type = "Task",
            Status = "To Do",
            Priority = "Medium"
        });
        var item = await createResponse.Content.ReadFromJsonAsync<BacklogItemResponse>();

        // Act
        var moveResponse = await Client.PatchAsJsonAsync(
            $"/api/workitems/{item!.Id}/move-to-board",
            new { BoardId = boardId });

        // Assert
        moveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify item is now on board
        var getResponse = await Client.GetAsync($"/api/boards/{boardId}/workitems/{item.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var movedItem = await getResponse.Content.ReadFromJsonAsync<BacklogItemResponse>();
        movedItem!.BoardId.Should().Be(boardId);
    }

    private record BacklogItemResponse(
        int Id,
        string Title,
        int? BoardId,
        int? BacklogOrder);
}
