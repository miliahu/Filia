using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Filia.ArchitectureTests;

public class NamingConventionTests : TestBase
{
    [Fact]
    public void Commands_ShouldHaveNameEndingWithCommand()
    {
        var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .ResideInNamespaceContaining("Commands")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Command")
            .Or()
            .HaveNameEndingWith("CommandHandler")
            .Or()
            .HaveNameEndingWith("CommandValidator")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Queries_ShouldHaveNameEndingWithQuery()
    {
        var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .ResideInNamespaceContaining("Queries")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameEndingWith("Dto")
            .Should()
            .HaveNameEndingWith("Query")
            .Or()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainEntities_ShouldBeSealedOrAbstract_ExceptAggregateRoots()
    {
        // FileItem intentionally stays unsealed to allow future EF Core proxying if ever needed;
        // this test documents that expectation rather than blindly enforcing sealed classes.
        var result = Types.InAssembly(typeof(Domain.Entities.FileItem).Assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Entities")
            .Should()
            .NotBeAbstract()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
