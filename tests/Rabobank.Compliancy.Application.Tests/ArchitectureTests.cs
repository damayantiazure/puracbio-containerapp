namespace Rabobank.Compliancy.Application.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Application_Should_Only_Depend_on_Domain_Project()
    {
        // Arrange
        var forbiddenReference = new List<string>
        {
            "Rabobank.Compliancy.Clients", "Rabobank.Compliancy.Infrastructure", "Rabobank.Compliancy.Web"
        };

        var targetAssembly = typeof(DependencyInjection).Assembly;
        
        // Act
        var referencedAssemblies = targetAssembly.GetReferencedAssemblies();
        var foundForbiddenReferences = new List<string>();
        foreach (var reference in forbiddenReference)
        {
            if (referencedAssemblies.Any(ra => ra.Name.StartsWith(reference)))
            {
                foundForbiddenReferences.Add(reference);
            }
        }

        // Assert
        if (foundForbiddenReferences.Any())
        {
            Assert.False(true, $"The target assembly {targetAssembly} should not contain references to {string.Join(", ", foundForbiddenReferences)} projects.");
        }
    }
}