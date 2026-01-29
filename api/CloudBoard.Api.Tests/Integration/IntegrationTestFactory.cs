using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using CloudBoard.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CloudBoard.Api.Tests.Integration;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // 1. Define the container here so it's shared
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("cloudboard_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    // 2. Start the container once when the Factory is created
    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    // 3. Configure the WebHost (moved from IntegrationTestBase)
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Jwt:Secret"] = "TestSecretKeyForIntegrationTestsThatIsLongEnough123456789", 
                ["Jwt:Issuer"] = "CloudBoardTestIssuer",
                ["Jwt:Audience"] = "CloudBoardTestAudience"
            }!);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext
            var descriptor = services.SingleOrDefault(d => 
                d.ServiceType == typeof(DbContextOptions<CloudBoardContext>));
            
            if (descriptor != null) 
                services.Remove(descriptor);

            // Add the Test Database using the container connection string
            services.AddDbContext<CloudBoardContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString(), o =>
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
        });
    }

    // 4. Dispose the container when all tests are done
    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}