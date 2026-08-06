using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using RealTest.Api.Extensions;
using RealTest.Api.Middleware;
using RealTest.Application;
using RealTest.Infrastructure;
using RealTest.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting RealTest API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Fail fast if required configuration is missing, rather than surfacing a confusing error later.
    var connectionString = builder.Configuration.GetConnectionString("Postgres");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Required configuration 'ConnectionStrings:Postgres' is missing. Set it via appsettings, " +
            "an environment variable (ConnectionStrings__Postgres), or .env when running under Docker Compose.");
    }

    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddApiAuthentication(builder.Configuration);
    builder.Services.AddAuthorization();
    builder.Services.AddApiCors(builder.Configuration);
    builder.Services.AddApiHealthChecks(builder.Configuration);
    builder.Services.AddApiOpenApi();

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    var app = builder.Build();

    await DatabaseInitializer.MigrateAndSeedAsync(app.Services);

    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "RealTest API v1"));
    }
    else
    {
        app.UseHttpsRedirection();
    }

    app.UseCors("Frontend");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = HealthCheckPredicates.Ready });
    app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = HealthCheckPredicates.Live });

    Log.Information("RealTest API started successfully.");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "RealTest API terminated unexpectedly during startup.");
}
finally
{
    Log.CloseAndFlush();
}

namespace RealTest.Api
{
    /// <summary>Exposed so WebApplicationFactory&lt;Program&gt; can bootstrap the app in integration tests.</summary>
    public partial class Program;
}
