using AutoFixture.Kernel;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;

public class IdentityIsAlwaysUser : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customizations.Add(
            new TypeRelay(
                typeof(Domain.Compliancy.Authorizations.IIdentity),
                typeof(User)));
    }
}