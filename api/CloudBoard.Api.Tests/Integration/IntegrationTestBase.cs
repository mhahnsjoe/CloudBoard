using System.Net.Http.Headers;
using System.Net.Http.Json;
using CloudBoard.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace CloudBoard.Api.Tests.Integration;

/// <summary>
/// Base class for integration tests using real PostgreSQL container.
/// Provides authenticated HTTP client and database access.
/// </summary>
public class IntegrationTestBase : IAsyncLifetime
{
    protected HttpClient Client { get; private set; } = null!;
    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("cloudboard_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Add test configuration including JWT secret
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Jwt:Secret"] = "TestSecretKeyForIntegrationTestsThatIsLongEnough123456789",
                        ["Jwt:Issuer"] = "CloudBoardTestIssuer",
                        ["Jwt:Audience"] = "CloudBoardTestAudience"
                    }!);
                });

                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<CloudBoardContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add test database
                    services.AddDbContext<CloudBoardContext>(options =>
                        options.UseNpgsql(_postgres.GetConnectionString()));
                });
            });

        Client = Factory.CreateClient();

        // Initialize database
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CloudBoardContext>();
        await context.Database.EnsureCreatedAsync();

        // Create test user and get auth token
        await SetupAuthenticationAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    /// <summary>
    /// Sets up authentication by registering a test user and obtaining JWT token.
    /// </summary>
    private async Task SetupAuthenticationAsync()
    {
        // Register test user
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "integration@test.com",
            Password = "TestPassword123!",
            Name = "Integration Test"
        });

        if (!registerResponse.IsSuccessStatusCode)
        {
            // User might already exist, try login
        }

        // Login to get token
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "integration@test.com",
            Password = "TestPassword123!"
        });

        loginResponse.EnsureSuccessStatusCode();

        var authResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResult?.Token);
    }

    /// <summary>
    /// Creates a project and returns its ID.
    /// </summary>
    protected async Task<int> CreateTestProjectAsync(string name = "Test Project")
    {
        var response = await Client.PostAsJsonAsync("/api/projects", new
        {
            Name = name,
            Description = "Integration test project"
        });
        response.EnsureSuccessStatusCode();

        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        return project!.Id;
    }

    /// <summary>
    /// Gets the default board for a project.
    /// </summary>
    protected async Task<int> GetDefaultBoardAsync(int projectId)
    {
        var response = await Client.GetAsync($"/api/projects/{projectId}/boards");
        response.EnsureSuccessStatusCode();

        var boards = await response.Content.ReadFromJsonAsync<List<BoardResponse>>();
        return boards!.First().Id;
    }

    // Response DTOs for deserialization
    private record AuthResponse(string Token, UserResponse User);
    private record UserResponse(int Id, string Email, string Name);
    protected record ProjectResponse(int Id, string Name, string Description);
    protected record BoardResponse(int Id, string Name, int ProjectId);
}
