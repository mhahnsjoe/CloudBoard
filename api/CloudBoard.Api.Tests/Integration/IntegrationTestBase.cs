using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CloudBoard.Api.Data;
using CloudBoard.Api.Models.DTO;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CloudBoard.Api.Tests.Integration;

[Collection("Integration")]
public class IntegrationTestBase : IAsyncLifetime
{
    protected readonly IntegrationTestFactory Factory;
    protected HttpClient Client { get; private set; } = null!;
    protected int PrimaryTeamId { get; private set; }

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public IntegrationTestBase(IntegrationTestFactory factory)
    {
        Factory = factory;
        Client = Factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Ensure Database Schema exists
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CloudBoardContext>();
        await context.Database.EnsureCreatedAsync();

        // Authenticate
        await AuthenticateAsync("integration@test.com", "TestPassword123!", "Integration Test");

        // Create a default team for this user
        var teamResponse = await Client.PostAsJsonAsync("/api/v1/teams", new { Name = "Primary Test Team" });
        teamResponse.EnsureSuccessStatusCode();
        var team = await teamResponse.Content.ReadFromJsonAsync<TeamDto>(JsonOptions);
        PrimaryTeamId = team!.Id;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task AuthenticateAsync(string email, string password = "Password123!", string name = "Test User")
    {
        var registerResponse = await Client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Password = password,
            Name = name
        });

        // If it fails, we assume user exists, try login
        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = email,
            Password = password
        });

        if (!loginResponse.IsSuccessStatusCode)
        {
            var body = await loginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Authentication failed! Status: {loginResponse.StatusCode}. URL: {Client.BaseAddress}api/v1/auth/login. Body: {body}");
        }

        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResult?.Token);
    }

    protected async Task<int> CreateTestProjectAsync(string name = "Test Project", int? teamId = null)
    {
        if (teamId == null)
        {
            var teamName = $"Team for {name}";
            var teamResponse = await Client.PostAsJsonAsync("/api/v1/teams", new { Name = teamName });
            
            if (!teamResponse.IsSuccessStatusCode)
            {
                 var body = await teamResponse.Content.ReadAsStringAsync();
                 throw new Exception($"Failed to create prerequisite team. Status: {teamResponse.StatusCode}, Body: {body}");
            }
            
            var team = await teamResponse.Content.ReadFromJsonAsync<TeamDto>(JsonOptions);
            teamId = team!.Id;
        }

        var response = await Client.PostAsJsonAsync("/api/v1/projects", new
        {
            Name = name,
            Description = "Integration test project",
            TeamId = teamId
        });
        
        if (!response.IsSuccessStatusCode)
        {
             var body = await response.Content.ReadAsStringAsync();
             throw new Exception($"Failed to create project. Status: {response.StatusCode}, Body: {body}");
        }

        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>(JsonOptions);
        return project!.Id;
    }

    protected async Task<int> GetDefaultBoardAsync(int projectId)
    {
        var response = await Client.GetAsync($"/api/v1/projects/{projectId}/boards");
        response.EnsureSuccessStatusCode();

        var boards = await response.Content.ReadFromJsonAsync<List<BoardResponse>>(JsonOptions);
        return boards!.First().Id;
    }

    private record AuthResponse(string Token, UserResponse User);
    private record UserResponse(int Id, string Email, string Name);
    protected record ProjectResponse(int Id, string Name, string Description);
    protected record BoardResponse(int Id, string Name, int ProjectId);
}