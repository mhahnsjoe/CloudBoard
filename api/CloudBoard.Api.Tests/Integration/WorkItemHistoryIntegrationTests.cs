using System.Net;
using System.Net.Http.Json;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CloudBoard.Api.Tests.Integration;

[Collection("Integration")]
public class WorkItemHistoryIntegrationTests : IntegrationTestBase
{


    public WorkItemHistoryIntegrationTests(IntegrationTestFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateWorkItem_ShouldRecordHistory()
    {
        // 1. Setup - Create project and board
        var projectId = await CreateTestProjectAsync("History Test Project");
        var boardId = await GetDefaultBoardAsync(projectId);

        // Create a work item
        var createResponse = await Client.PostAsJsonAsync($"/api/v1/boards/{boardId}/workitems", new WorkItemCreateDto
        {
            Title = "Record History Test",
            Type = WorkItemType.Task,
            Status = "To Do",
            BoardId = boardId,
            ProjectId = projectId
        });
        createResponse.EnsureSuccessStatusCode();
        var workItem = await createResponse.Content.ReadFromJsonAsync<WorkItemResponse>(JsonOptions);
        workItem.Should().NotBeNull();

        // 2. Act - Update work item status
        var updateDto = new WorkItemUpdateDto
        {
            Title = "Updated Title",
            Status = "In Progress",
            Priority = workItem!.Priority,
            Type = Enum.Parse<WorkItemType>(workItem.Type),
            BoardId = boardId
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/v1/boards/{boardId}/workitems/{workItem.Id}", updateDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 3. Assert - Check history records in database
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CloudBoard.Api.Data.CloudBoardContext>();
        
        var history = context.WorkItemHistories
            .Where(h => h.WorkItemId == workItem.Id && h.FieldName == "Status")
            .ToList();

        history.Should().NotBeEmpty();
        var statusChange = history.FirstOrDefault(h => h.NewValue == "In Progress");
        statusChange.Should().NotBeNull();
        statusChange!.OldValue.Should().Be("To Do");
    }

    [Fact]
    public async Task AssignToSprint_ShouldRecordHistory()
    {
        // 1. Setup
        var projectId = await CreateTestProjectAsync("Sprint History Project");
        var boardId = await GetDefaultBoardAsync(projectId);

        // Create sprint
        var sprintResponse = await Client.PostAsJsonAsync($"/api/v1/boards/{boardId}/sprints", new CreateSprintDto
        {
            Name = "History Sprint",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14)
        });
        sprintResponse.EnsureSuccessStatusCode();
        var sprint = await sprintResponse.Content.ReadFromJsonAsync<SprintResponse>(JsonOptions);

        // Create work item
        var createResponse = await Client.PostAsJsonAsync($"/api/v1/boards/{boardId}/workitems", new WorkItemCreateDto
        {
            Title = "Sprint History Item",
            Type = WorkItemType.Task,
            Status = "To Do",
            BoardId = boardId,
            ProjectId = projectId
        });
        var workItem = await createResponse.Content.ReadFromJsonAsync<WorkItemResponse>(JsonOptions);

        // 2. Act - Assign to sprint via bulk operation (Correct endpoint)
        var assignResponse = await Client.PostAsJsonAsync($"/api/v1/sprints/{sprint!.Id}/items/assign", new BulkSprintAssignmentDto 
        { 
            WorkItemIds = new List<int> { workItem!.Id } 
        });
        assignResponse.EnsureSuccessStatusCode();

        // 3. Assert
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CloudBoard.Api.Data.CloudBoardContext>();
        
        var history = context.WorkItemHistories
            .Where(h => h.WorkItemId == workItem.Id && h.FieldName == "SprintId")
            .ToList();

        history.Should().NotBeEmpty();
        history.Any(h => h.NewValue == sprint.Id.ToString()).Should().BeTrue();
    }

    private record WorkItemResponse(int Id, string Title, string Type, string Status, string Priority);
    private record SprintResponse(int Id, string Name, string Status);
}
