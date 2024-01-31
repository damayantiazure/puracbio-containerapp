using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;

public class ProjectWithoutPermissions : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Project>(project => project.Without(p => p.Permissions));
    }
}