using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using RealTest.Application.Authentication;
using RealTest.Application.Common.Security;

namespace RealTest.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton<IAuthorizationHandler, ActiveAccountRequirementHandler>();

        return services;
    }
}
