using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Tests;

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