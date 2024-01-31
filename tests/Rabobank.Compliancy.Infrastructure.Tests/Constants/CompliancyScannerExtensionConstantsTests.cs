using Rabobank.Compliancy.Infrastructure.Constants;

namespace Rabobank.Compliancy.Infrastructure.Tests.Constants;

public class CompliancyScannerExtensionConstantsTests
{
    [Fact]
    public void Publisher_ShouldNeverChange()
    {
        CompliancyScannerExtensionConstants.Publisher.Should().Be("tas");
    }

    [Fact]
    public void Collection_ShouldNeverChange()
    {
        CompliancyScannerExtensionConstants.Collection.Should().Be("compliancy");
    }
}