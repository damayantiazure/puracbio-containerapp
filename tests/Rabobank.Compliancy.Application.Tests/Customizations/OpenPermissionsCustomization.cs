using AutoFixture.Kernel;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure;

namespace Rabobank.Compliancy.Application.Tests.Customizations;

public class OpenPermissionsCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customizations.Add(
            new TypeRelay(typeof(Domain.Compliancy.Authorizations.IIdentity), typeof(User)));

        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        fixture.Customizations.Add(
            new TypeRelay(typeof(PipelineResource), typeof(Pipeline)));

        fixture.Customizations.Add(
           new TypeRelay(typeof(ITrigger), typeof(PipelineTrigger)));

        fixture.Customizations.Add(
            new TypeRelay(typeof(ISettings), typeof(Pipeline)));
    }
}