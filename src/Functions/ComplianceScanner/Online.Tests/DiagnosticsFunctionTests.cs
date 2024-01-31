using Microsoft.AspNetCore.Mvc;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class DiagnosticsFunctionTests
{
    [Fact]
    public void Run_ShouldReturnOk()
    {
        var sut = new DiagnosticsFunction();
        var actual = sut.Run(null);
        Assert.True(actual is OkObjectResult);
    }
}