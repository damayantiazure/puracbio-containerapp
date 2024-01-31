using Rabobank.Compliancy.Application.Reconcile;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Application.Tests.ProcessTestImplementations.Reconcile;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Application.Tests.Reconcile;

public class ReconcileProcessBaseTests
{
    private readonly ReconcileProcessBaseTestImplementation _sut;
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly ReconcileRequest _requestDefault;
    private readonly Project _projectDefault;

    public ReconcileProcessBaseTests()
    {
        _sut = new ReconcileProcessBaseTestImplementation(_projectServiceMock.Object, _compliancyReportServiceMock.Object);
        _fixture.Customize(new IdentityIsAlwaysUser());
        _requestDefault = _fixture.Create<ReconcileRequest>();
        _projectDefault = _fixture.Create<Project>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateReportAsync_ShouldUpdateReport_WithReevaluationResult(bool reevaluationResult)
    {
        // Arrange
        _projectServiceMock.Setup(mock => mock.GetProjectByIdAsync(_requestDefault.Organization, _requestDefault.ProjectId, default))
            .ReturnsAsync(_projectDefault)
            .Verifiable();

        _compliancyReportServiceMock.Setup(mock => mock.UpdateComplianceStatusAsync(_projectDefault, _requestDefault.ProjectId, _requestDefault.ItemId, _requestDefault.RuleName, It.IsAny<bool>(), reevaluationResult))
            .Verifiable();

        // Act
        await _sut.UsesUpdateReportAsync(_requestDefault, reevaluationResult, default);

        // Assert
        _projectServiceMock.Verify();
        _compliancyReportServiceMock.Verify();
    }
}