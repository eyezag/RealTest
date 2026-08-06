using System.Reflection;
using Shouldly;
using Xunit;

namespace RealTest.ArchitectureTests;

/// <summary>
/// Guards the modular-monolith layering rules by inspecting each assembly's actual referenced
/// assemblies (not just source-level `using` directives, which wouldn't catch a stray
/// ProjectReference). If one of these ever goes red, a project reference was added that breaks
/// the intended dependency direction: Domain -> (nothing), Application -> Domain/Contracts,
/// Infrastructure -> Application/Domain, Api -> everything, Contracts -> (nothing).
/// </summary>
public sealed class LayerDependencyTests
{
    private static readonly Assembly DomainAssembly = typeof(RealTest.Domain.Users.ApplicationUser).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(RealTest.Application.Authentication.IAuthService).Assembly;
    private static readonly Assembly ContractsAssembly = typeof(RealTest.Contracts.Auth.RegisterRequest).Assembly;

    private static bool ReferencesAssembly(Assembly assembly, string referencedAssemblySimpleName) =>
        assembly.GetReferencedAssemblies().Any(a => a.Name == referencedAssemblySimpleName);

    [Fact]
    public void Domain_DoesNotReferenceInfrastructure()
    {
        ReferencesAssembly(DomainAssembly, "RealTest.Infrastructure").ShouldBeFalse();
    }

    [Fact]
    public void Domain_DoesNotReferenceApi()
    {
        ReferencesAssembly(DomainAssembly, "RealTest.Api").ShouldBeFalse();
    }

    [Fact]
    public void Domain_DoesNotReferenceApplication()
    {
        // Dependencies must point inward only: Application depends on Domain, never the reverse.
        ReferencesAssembly(DomainAssembly, "RealTest.Application").ShouldBeFalse();
    }

    [Fact]
    public void Application_DoesNotReferenceApi()
    {
        ReferencesAssembly(ApplicationAssembly, "RealTest.Api").ShouldBeFalse();
    }

    [Fact]
    public void Application_DoesNotReferenceInfrastructure()
    {
        // Application defines the interfaces; Infrastructure implements them — never the reverse.
        ReferencesAssembly(ApplicationAssembly, "RealTest.Infrastructure").ShouldBeFalse();
    }

    [Fact]
    public void Contracts_DoesNotReferenceInfrastructure()
    {
        ReferencesAssembly(ContractsAssembly, "RealTest.Infrastructure").ShouldBeFalse();
    }

    [Fact]
    public void Contracts_DoesNotReferenceApi()
    {
        ReferencesAssembly(ContractsAssembly, "RealTest.Api").ShouldBeFalse();
    }

    [Fact]
    public void Contracts_DoesNotReferenceApplication()
    {
        // Contracts are pure wire-format DTOs with no dependency on any other RealTest project.
        ReferencesAssembly(ContractsAssembly, "RealTest.Application").ShouldBeFalse();
    }

    [Fact]
    public void Contracts_DoesNotReferenceDomain()
    {
        ReferencesAssembly(ContractsAssembly, "RealTest.Domain").ShouldBeFalse();
    }
}
