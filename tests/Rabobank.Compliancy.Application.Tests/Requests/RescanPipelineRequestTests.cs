using Rabobank.Compliancy.Application.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Application.Tests.Requests;

public class RescanPipelineRequestTests
{
    [Fact]
    public void ToExceptionReport_RescanPipelineRequestWithData_CreatesCorrectExceptionReport()
    {
        // Arrange            
        const string functionName = "unittest";
        const string functionUrl = "http://www.unittest.nl";
        var exception = new InvalidOperationException("dummy message");
        const string projectId = "af3248fc-bd56-4f36-9a21-b23f27318b9a";

        var request = new RescanPipelineRequest
        {
            Organization = "raboweb-test",
            PipelineId = 10,
            ProjectId = new Guid(projectId)
        };

        // Act
        var exceptionReport = request.ToExceptionReport(functionName, functionUrl, exception);

        // Assert
        Assert.Equal(functionName, exceptionReport.FunctionName);
        Assert.Equal(functionUrl, exceptionReport.RequestUrl);
        Assert.Equal(request.Organization, exceptionReport.Organization);
        Assert.Equal(projectId, exceptionReport.ProjectId);
    }
}