using System.Net.Http.Json;
using CloudBoard.Api.Data;
using CloudBoard.Api.Models;
using CloudBoard.Api.Models.DTO;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CloudBoard.Api.Tests.Integration;

public class TeamsIntegrationTests : IntegrationTestBase
{
    public TeamsIntegrationTests(IntegrationTestFactory factory) : base(factory)
    {
    }

    private async Task<string> GetResponseBody(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task CreateTeam_ShouldCreateAndReturnTeam_WhenDataIsValid()
    {
        // Arrange
        await AuthenticateAsync("user1@test.com", "Password123!", "user1");
        var command = new CreateTeamDto
        {
            Name = "Integration Test Team",
            Description = "Created via integration test"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/teams", command);

        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var body = await GetResponseBody(response);
            throw new Exception($"POST /api/v1/teams failed. Status: {response.StatusCode}, Body: {body}");
        }

        var team = await response.Content.ReadFromJsonAsync<TeamDto>(JsonOptions);
        
        team.Should().NotBeNull();
        team!.Name.Should().Be(command.Name);
        team.CurrentUserRole.Should().Be(TeamRole.Owner);

        // Verify in DB
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CloudBoardContext>();
        var dbTeam = await db.Teams.FindAsync(team.Id);
        dbTeam.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTeam_ShouldReturnDetails_WhenUserIsMember()
    {
        // Arrange
        await AuthenticateAsync("user2@test.com", "Password123!", "user2");
        var createResponse = await Client.PostAsJsonAsync("/api/v1/teams", new CreateTeamDto { Name = "Detail Test Team" });
        createResponse.EnsureSuccessStatusCode();
        var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>(JsonOptions);

        // Act
        var response = await Client.GetAsync($"/api/v1/teams/{team!.Id}");

        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var body = await GetResponseBody(response);
            throw new Exception($"GET /api/v1/teams/{team.Id} failed. Status: {response.StatusCode}, Body: {body}");
        }

        var detail = await response.Content.ReadFromJsonAsync<TeamDetailDto>(JsonOptions);
        
        detail.Should().NotBeNull();
        detail!.Id.Should().Be(team.Id);
        detail.Members.Should().Contain(m => m.Name == "user2" && m.Role == TeamRole.Owner);
    }

    [Fact]
    public async Task InviteMember_ShouldCreateInvitation_WhenUserIsAdmin()
    {
        // Arrange
        await AuthenticateAsync("user3@test.com", "Password123!", "user3");
        // Create team (user3 is Owner/Admin)
        var createResponse = await Client.PostAsJsonAsync("/api/v1/teams", new CreateTeamDto { Name = "Invite Team" });
        createResponse.EnsureSuccessStatusCode();
        var team = await createResponse.Content.ReadFromJsonAsync<TeamDto>(JsonOptions);

        var inviteDto = new InviteMemberDto
        {
            Email = "newuser@test.com",
            Role = TeamRole.Member
        };

        // Act - Note the correct route from TeamsController: {id}/invitations
        var response = await Client.PostAsJsonAsync($"/api/v1/teams/{team!.Id}/invitations", inviteDto);

        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var body = await GetResponseBody(response);
            throw new Exception($"POST /api/v1/teams/{team.Id}/invitations failed. Status: {response.StatusCode}, Body: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<TeamInvitationDto>(JsonOptions);
        
        result.Should().NotBeNull();
        result!.Email.Should().Be(inviteDto.Email);
        result.Token.Should().NotBeNullOrEmpty();
    }
}
