using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RealTest.Application.Common.Security;
using RealTest.Domain.Enums;
using RealTest.Infrastructure.Authentication;

namespace RealTest.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("Authentication");
                        logger.LogWarning("JWT authentication failed: {Reason}", context.Exception.Message);
                        return Task.CompletedTask;
                    }
                };
            });

        // Bound lazily from the same IOptions<JwtOptions> that RealTest.Infrastructure's
        // TokenService reads when *signing* tokens, instead of re-reading IConfiguration
        // eagerly here. Reading configuration eagerly at registration time (rather than
        // through the Options pattern) can observe a different snapshot than what the rest
        // of the app resolves later — e.g. under WebApplicationFactory in tests, where
        // configuration overrides are layered in after this method runs. Binding through
        // IOptions<JwtOptions> guarantees the signing key used to validate a token always
        // matches the one used to issue it.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptions) =>
            {
                var options = jwtOptions.Value;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("TesterOnly", policy => policy
                .RequireRole(nameof(UserType.Tester))
                .AddRequirements(new ActiveAccountRequirement()))
            .AddPolicy("CompanyUserOnly", policy => policy
                .RequireRole(nameof(UserType.CompanyUser))
                .AddRequirements(new ActiveAccountRequirement()))
            .AddPolicy("AdministratorOnly", policy => policy
                .RequireRole(nameof(UserType.Administrator))
                .AddRequirements(new ActiveAccountRequirement()));

        return services;
    }

    public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
    {
        var frontendUrl = configuration["Frontend:Url"] ?? "http://localhost:3000";

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy => policy
                .WithOrigins(frontendUrl)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });

        return services;
    }

    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql", tags: ["ready"]);

        return services;
    }

    public static IServiceCollection AddApiOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RealTest API",
                Version = "v1",
                Description = "Authentication and account foundation for the RealTest tester/company marketplace."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter the access token returned by /api/v1/auth/login or /register."
            });

            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", null),
                    []
                }
            });
        });

        return services;
    }
}

/// <summary>Health check predicate helpers used when mapping /health endpoints in Program.cs.</summary>
public static class HealthCheckPredicates
{
    public static readonly Func<HealthCheckRegistration, bool> Ready = check => check.Tags.Contains("ready");
    public static readonly Func<HealthCheckRegistration, bool> Live = _ => false;
}
