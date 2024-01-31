#nullable enable

using AutoFixture.Kernel;
using System.Reflection;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.FixtureCustomizations;

/// <inheritdoc/>
public class NonPublicConstructorBuilder<T> : ISpecimenBuilder where T : class
{
    /// <inheritdoc/>
    public object Create(object request, ISpecimenContext context)
    {
        var type = request as Type;
        if (type != null && type == typeof(T))
        {
            var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                null, Type.EmptyTypes, null);
            if (constructor != null)
            {
                return constructor.Invoke(null);
            }
        }

        return new NoSpecimen();
    }
}