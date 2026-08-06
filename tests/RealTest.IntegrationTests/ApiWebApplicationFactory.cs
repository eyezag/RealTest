using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using RealTest.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace RealTest.IntegrationTests;

/// <summary>
/// Boots the real Api host against a disposable PostgreSQL container (never EF Core's
/// in-memory provider) so integration tests exercise the actual EF Core/Npgsql behavior —
/// including the things the in-memory provider can't verify, like unique indexes and
/// PostgreSQL's xmin-based concurrency tokens.
/// </summary>
public sealed class ApiWebApplicationFactory : WebApplicationFactory<Api.Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("realtest_test")
        .WithUsername("realtest_test")
        .WithPassword("realtest_test")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _dbContainer.GetConnectionString(),
                ["Jwt:Issuer"] = "realtest-integration-tests",
                ["Jwt:Audience"] = "realtest-integration-tests",
                ["Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-characters-long",
                ["Jwt:AccessTokenLifetimeMinutes"] = "15",
                ["Jwt:RefreshTokenLifetimeDays"] = "7",
                ["Frontend:Url"] = "http://localhost:3000"
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // WebApplicationFactory's minimal-hosting integration deliberately unwinds Program's
        // top-level statements right after WebApplicationBuilder.Build(), so the migration/seed
        // call that normally runs in Program.cs never executes under the test host. Run it
        // explicitly here instead, now that Services reflects the container's connection string.
        await DatabaseInitializer.MigrateAndSeedAsync(Services);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await base.DisposeAsync();
    }
}
