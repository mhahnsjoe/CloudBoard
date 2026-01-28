using System.Net.Http.Headers;
using System.Net.Http.Json;
using CloudBoard.Api.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CloudBoard.Api.Tests.Integration;

[Collection("Integration")]
public class IntegrationTestBase : IAsyncLifetime
{
    private readonly IntegrationTestFactory _factory;
    protected HttpClient Client { get; private set; } = null!;

    public IntegrationTestBase(IntegrationTestFactory factory)
    {
        _factory = factory;
        Client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Ensure Database Schema exists
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CloudBoardContext>();
        await context.Database.EnsureCreatedAsync();

        // Authenticate
        // Note: Since data persists between tests in this model, 
        // we handle the case where the user might already exist.
        await SetupAuthenticationAsync();
    }

    public Task DisposeAsync()
    {
        // We do NOT dispose the factory here anymore.
        // It is managed by the Collection Fixture.
        return Task.CompletedTask;
    }

    private async Task SetupAuthenticationAsync()
    {
        var registerResponse = await Client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = "integration@test.com",
            Password = "TestPassword123!",
            Name = "Integration Test"
        });

        // If it fails, we assume it's because the user already exists
        // from a previous test run in this session.

        var loginResponse = await Client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = "integration@test.com",
            Password = "TestPassword123!"
        });

        loginResponse.EnsureSuccessStatusCode();

        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResult?.Token);
    }

    protected async Task<int> CreateTestProjectAsync(string name = "Test Project")
    {
        var response = await Client.PostAsJsonAsync("/api/v1/projects", new
        {
            Name = name,
            Description = "Integration test project"
        });
        response.EnsureSuccessStatusCode();

        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        return project!.Id;
    }

    protected async Task<int> GetDefaultBoardAsync(int projectId)
    {
        var response = await Client.GetAsync($"/api/v1/projects/{projectId}/boards");
        response.EnsureSuccessStatusCode();

        var boards = await response.Content.ReadFromJsonAsync<List<BoardResponse>>();
        return boards!.First().Id;
    }

    private record AuthResponse(string Token, UserResponse User);
    private record UserResponse(int Id, string Email, string Name);
    protected record ProjectResponse(int Id, string Name, string Description);
    protected record BoardResponse(int Id, string Name, int ProjectId);
}