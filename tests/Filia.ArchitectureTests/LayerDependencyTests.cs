using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Filia.ArchitectureTests;

/// <summary>
/// Enforces the Clean Architecture dependency rule: dependencies only point
/// inward. Domain knows nothing about anyone; Application knows only Domain;
/// Infrastructure and Api may depend on both, but never on each other.
/// </summary>
public class LayerDependencyTests : TestBase
{
    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherProjects()
    {
        var forbiddenLayers = new[] { ApplicationNamespace, InfrastructureNamespace, ApiNamespace };

        var result = Types.InAssembly(typeof(Domain.Entities.FileItem).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenLayers)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FormatFailures(result));
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnInfrastructureOrApi()
    {
        var forbiddenLayers = new[] { InfrastructureNamespace, ApiNamespace };

        var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenLayers)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FormatFailures(result));
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnEntityFrameworkCore()
    {
        // Enforces the repository-pattern boundary: Application talks to
        // persistence only through IFileRepository / IUnitOfWork. EF Core
        // itself is an Infrastructure-only implementation detail.
        var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FormatFailures(result));
    }

    [Fact]
    public void Infrastructure_ShouldNotHaveDependencyOnApi()
    {
        var result = Types.InAssembly(typeof(Infrastructure.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FormatFailures(result));
    }

    [Fact]
    public void Handlers_ShouldNotBePublic_UnlessTheyImplementInterface()
    {
        // Command/query handlers should only be reachable through MediatR (ISender),
        // never called directly across layers.
        var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .ResideInNamespaceContaining("Files")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FormatFailures(result));
    }

    [Fact]
    public void Controllers_ShouldOnlyDependOn_Application_Not_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Api.Controllers.FilesController).Assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(FormatFailures(result));
    }

    private static string FormatFailures(NetArchTest.Rules.TestResult result) =>
        result.FailingTypes is null
            ? "No details available."
            : string.Join(", ", result.FailingTypeNames);
}
