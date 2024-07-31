using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests.Services;

public class PipelineRegistratorTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ICmdbClient> _cmdbClientMock;
    private readonly Mock<IPipelineRegistrationStorageRepository> _pipelineRegistrationStorageMock;
    private readonly Mock<IPipelineRegistrationRepository> _pipelineRegistrationRepositoryMock;
    private readonly Mock<IManageHooksService> _manageHooksServiceMock;
    private readonly PipelineRegistrator _sut;
    private const string _validUserEmailAddress = "test@raboag.com";
    private const string _validPipelineType = ItemTypes.YamlReleasePipeline;
    private const string _validCiIdentifier = "C12345678";
    private readonly UserEntitlement _validUserEntitlement;
    private readonly AssignmentGroup _validAssignmentGroup;
    private readonly string _dummyOrganization;
    private readonly string _dummyProjectId;
    private readonly string _dummyPipelineId;
    private readonly string _dummyAssignmentGroup;
    private readonly string _dummyCiName;
    private readonly string _dummyStageId;
    private readonly RegistrationRequest _dummyInput;
    private readonly UserEntitlement _dummyUserEntitlement;

    public PipelineRegistratorTests()
    {
        _cmdbClientMock = new Mock<ICmdbClient>();
        _pipelineRegistrationStorageMock = new Mock<IPipelineRegistrationStorageRepository>();
        _pipelineRegistrationRepositoryMock = new Mock<IPipelineRegistrationRepository>();
        _manageHooksServiceMock = new Mock<IManageHooksService>();
        _sut = new PipelineRegistrator(_cmdbClientMock.Object, _pipelineRegistrationStorageMock.Object,
            _pipelineRegistrationRepositoryMock.Object, _manageHooksServiceMock.Object);

        _dummyOrganization = _fixture.Create<string>();
        _dummyProjectId = _fixture.Create<string>();
        _dummyPipelineId = _fixture.Create<string>();
        _dummyAssignmentGroup = _fixture.Create<string>();
        _dummyCiName = _fixture.Create<string>();
        _dummyStageId = _fixture.Create<string>();
        _dummyInput = _fixture.Create<RegistrationRequest>();
        _dummyUserEntitlement = _fixture.Create<UserEntitlement>();
        _validUserEntitlement = new UserEntitlement
        {
            User = new User
            {
                MailAddress = _validUserEmailAddress,
            }
        };
        _validAssignmentGroup = new AssignmentGroup
        {
            GroupMembers = new List<string>
            {
                _validUserEmailAddress,
                "upn@rabobank.com"
            }
        };
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_WhenValidUserAndCiThenRegisterInCmdbAndTableStorage_ReturnsOkResult()
    {
        // Arrange
        const string userEmailAddress = "test@raboag.com";
        var ci = new ConfigurationItem
        {
            CiID = _dummyInput.CiIdentifier,
            CiName = _dummyCiName,
            ConfigAdminGroup = _dummyAssignmentGroup,
            CiType = "application",
            Status = "In Use - Production",
            Environment = new List<string> { "Production" },
        };

        var assignment = new AssignmentGroup
        {
            GroupMembers = new List<string>
            {
                "test@raboag.com",
                "upn@rabobank.com"
            }
        };

        _cmdbClientMock.Setup(c => c.GetCiAsync(_dummyInput.CiIdentifier)).ReturnsAsync(ci);
        _cmdbClientMock.Setup(c => c.GetAssignmentGroupAsync(_dummyAssignmentGroup)).ReturnsAsync(assignment);
        _pipelineRegistrationRepositoryMock.Setup(x => x.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(new List<PipelineRegistration>());

        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();

        // Act
        var result = await _sut.RegisterProdPipelineAsync(organization, projectId, pipelineId, _validPipelineType, userEmailAddress, _dummyInput);

        // Assert
        _cmdbClientMock.Verify(cmdbClient => cmdbClient.GetCiAsync(_dummyInput.CiIdentifier), Times.Once);
        _cmdbClientMock.Verify(cmdbClient => cmdbClient.GetAssignmentGroupAsync(_dummyAssignmentGroup), Times.Once);
        _pipelineRegistrationStorageMock.Verify(pipelineRegistrationStorageRepository => pipelineRegistrationStorageRepository.DeleteEntitiesForPipelineAsync(null, It.IsAny<string>(), It.IsAny<string>(), _validPipelineType, It.IsAny<string>()), Times.Once);
        _cmdbClientMock.Verify(cmdbClient => cmdbClient.InsertDeploymentMethodAsync(It.Is<DeploymentMethod>(deploymentMethod => deploymentMethod.CiName == _dummyCiName)), Times.Once);
        _pipelineRegistrationStorageMock.Verify(pipelineRegistrationStorageRepository => pipelineRegistrationStorageRepository.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Once);
        _manageHooksServiceMock.Verify(manageHooksService => manageHooksService.CreateHookAsync(It.IsAny<string>(), projectId, _validPipelineType, pipelineId), Times.Once);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_WithNoExistingCi_ShouldReturnBadRequest()
    {
        // Arrange
        var ci = new ConfigurationItem();

        _cmdbClientMock.Setup(m => m.GetCiAsync(_dummyInput.CiIdentifier)).ReturnsAsync(ci);

        // Act
        var result = await _sut.RegisterProdPipelineAsync(_fixture.Create<string>(),
            _fixture.Create<string>(), _fixture.Create<string>(), _validPipelineType,
            _fixture.Create<string>(), _dummyInput);

        // Assert
        _cmdbClientMock.Verify(m => m.GetCiAsync(_dummyInput.CiIdentifier));
        _cmdbClientMock.Verify(m => m.InsertDeploymentMethodAsync(It.IsAny<DeploymentMethod>()), Times.Never);
        _pipelineRegistrationStorageMock.Verify(m => m.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Never);
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_IfCiStatusDoesNotStartWithInUse_ShouldReturnBadRequest()
    {
        // Arrange
        var ci = new ConfigurationItem
        {
            ConfigAdminGroup = _fixture.Create<string>(),
            Status = "Invalid",
            CiType = "application",
            Environment = new List<string> { "Production" },
            CiID = _dummyInput.CiIdentifier
        };

        _cmdbClientMock.Setup(m => m.GetCiAsync(_dummyInput.CiIdentifier)).ReturnsAsync(ci);

        // Act
        var result = await _sut.RegisterProdPipelineAsync(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _validPipelineType,
            _fixture.Create<string>(), _dummyInput);

        // Assert
        _cmdbClientMock.Verify(m => m.GetCiAsync(_dummyInput.CiIdentifier));
        _cmdbClientMock.Verify(m => m.InsertDeploymentMethodAsync(It.IsAny<DeploymentMethod>()), Times.Never);
        _pipelineRegistrationStorageMock.Verify(m => m.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Never);
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_IfCiTypeIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var ci = new ConfigurationItem
        {
            ConfigAdminGroup = _fixture.Create<string>(),
            Status = "In Use - Production",
            CiType = "Invalid",
            Environment = new List<string> { "Production" },
            CiID = _dummyInput.CiIdentifier
        };

        _cmdbClientMock.Setup(m => m.GetCiAsync(_dummyInput.CiIdentifier)).ReturnsAsync(ci);

        // Act
        var result = await _sut.RegisterProdPipelineAsync(_fixture.Create<string>(),
            _fixture.Create<string>(), _fixture.Create<string>(), _validPipelineType, _fixture.Create<string>(), _dummyInput);

        // Assert
        _cmdbClientMock.Verify(m => m.GetCiAsync(_dummyInput.CiIdentifier));
        _cmdbClientMock.Verify(m => m.InsertDeploymentMethodAsync(It.IsAny<DeploymentMethod>()), Times.Never);
        _pipelineRegistrationStorageMock.Verify(m => m.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Never);
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Theory]
    [InlineData(new object[] { new[] { "Invalid" } })]
    [InlineData(null)]
    public async Task RegisterProdPipelineAsync_IfCiEnvironmentIsNullOrNotProduction_ShouldReturnBadRequest(string[] environments)
    {
        // Arrange
        var ci = new ConfigurationItem
        {
            ConfigAdminGroup = _fixture.Create<string>(),
            Status = "In Use - Production",
            CiType = "application",
            Environment = environments
        };

        _cmdbClientMock.Setup(m => m.GetCiAsync(_dummyInput.CiIdentifier)).ReturnsAsync(ci);

        // Act
        var result = await _sut.RegisterProdPipelineAsync(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _validPipelineType,
            _fixture.Create<string>(), _dummyInput);

        // Assert
        _cmdbClientMock.Verify(m => m.GetCiAsync(_dummyInput.CiIdentifier));
        _cmdbClientMock.Verify(m => m.InsertDeploymentMethodAsync(It.IsAny<DeploymentMethod>()), Times.Never);
        _pipelineRegistrationStorageMock.Verify(m => m.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Never);
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_UnauthorizedIfUserHasNoPermission_ShouldReturn()
    {
        // Arrange
        var userId1 = _fixture.Create<string>();
        var userId2 = _fixture.Create<string>();

        var ci = new ConfigurationItem
        {
            ConfigAdminGroup = _dummyAssignmentGroup,
            Status = "In Use - Production",
            CiType = "application",
            Environment = new List<string> { "Production" },
            CiID = _dummyInput.CiIdentifier
        };
        var assignment = new AssignmentGroup
        {
            GroupMembers = new List<string>
            {
                userId1
            }
        };
        
        _cmdbClientMock.Setup(m => m.GetCiAsync(_dummyInput.CiIdentifier)).ReturnsAsync(ci);
        _cmdbClientMock.Setup(m => m.GetAssignmentGroupAsync(_dummyAssignmentGroup)).ReturnsAsync(assignment);

        // Act
        var result = await _sut.RegisterProdPipelineAsync(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _validPipelineType,
            userId2, _dummyInput);

        // Assert
        _cmdbClientMock.Verify(m => m.GetCiAsync(_dummyInput.CiIdentifier));
        _cmdbClientMock.Verify(m => m.GetAssignmentGroupAsync(_dummyAssignmentGroup));
        _cmdbClientMock.Verify(m => m.InsertDeploymentMethodAsync(It.IsAny<DeploymentMethod>()), Times.Never);
        _pipelineRegistrationStorageMock.Verify(m => m.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Never);
        result.ShouldBeOfType(typeof(UnauthorizedObjectResult));
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_IfCiIsAlreadyRegistered_ShouldReturnBadRequest()
    {
        // Arrange
        const string userMail = "test@rabobank.com";

        var deploymentInfo = new DeploymentInformation
        {
            Information = $"{{\"organization\":\"{_dummyOrganization}\",\"project\":\"{_dummyProjectId}\",\"pipeline\":\"{_dummyPipelineId}\",\"stage\":\"{_dummyInput.Environment}\"}}",
            Method = "Azure Devops"
        };

        var ci = new ConfigurationItem
        {
            ConfigAdminGroup = _dummyAssignmentGroup,
            Status = "In Use - Production",
            CiType = "application",
            Environment = new List<string> { "Production" },
            CiID = _dummyInput.CiIdentifier,
            CiName = _dummyCiName
        };

        var assignment = new AssignmentGroup
        {
            GroupMembers = new List<string>
            {
                userMail
            }
        };

        _cmdbClientMock.Setup(m => m.GetCiAsync(_dummyInput.CiIdentifier)).ReturnsAsync(ci);
        _cmdbClientMock.Setup(m => m.GetAssignmentGroupAsync(_dummyAssignmentGroup)).ReturnsAsync(assignment);
        _cmdbClientMock.Setup(m => m.GetDeploymentMethodAsync(_dummyCiName))
            .ReturnsAsync(new List<DeploymentInformation> { deploymentInfo });

        // Act
        var result = await _sut.RegisterProdPipelineAsync(
            _dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, userMail, _dummyInput);

        // Assert
        _cmdbClientMock.Verify(m => m.GetCiAsync(_dummyInput.CiIdentifier));
        _cmdbClientMock.Verify(m => m.InsertDeploymentMethodAsync(It.IsAny<DeploymentMethod>()), Times.Never);
        _pipelineRegistrationStorageMock.Verify(m => m.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Never);
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        result.ShouldBeEquivalentTo(new BadRequestObjectResult(
            $"The registration failed with a bad request error, " +
            $"because an identical pipeline registration already exists. " +
            $"Please use the rescan button to update your compliance page."
        ));
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_WithInvalidConfigurationItem_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        _cmdbClientMock.Setup(cmdbClient => cmdbClient.GetCiAsync(It.Is<string>(s => s == _dummyInput.CiIdentifier)))
            .ReturnsAsync((ConfigurationItem)null);

        // Act
        var actual = await _sut.RegisterProdPipelineAsync(
            _dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _dummyUserEntitlement.User?.MailAddress, _dummyInput);

        // Assert
        actual.ShouldBeEquivalentTo(new BadRequestObjectResult(ErrorMessages.CiDoesNotExist(_dummyInput.CiIdentifier)));
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_WithInvalidType_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var configurationItem = _fixture.Create<ConfigurationItem>();
        _cmdbClientMock.Setup(cmdbClient => cmdbClient.GetCiAsync(It.Is<string>(s => s == _dummyInput.CiIdentifier)))
            .ReturnsAsync(configurationItem);

        // Act
        var actual = await _sut.RegisterProdPipelineAsync(
            _dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _dummyUserEntitlement.User?.MailAddress, _dummyInput);

        // Assert
        actual.ShouldBeEquivalentTo(new BadRequestObjectResult(
            $"The registration failed with a bad request error, " +
            $"because the Configuration Item {configurationItem.CiID} is invalid. " +
            $"Please make sure the CI Type is 'application' or 'subapplication'."
        ));
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_WithInvalidEnvironment_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var configurationItem = _fixture.Build<ConfigurationItem>()
            .With(item => item.CiType, "application").Create();
        _cmdbClientMock.Setup(cmdbClient => cmdbClient.GetCiAsync(It.Is<string>(s => s == _dummyInput.CiIdentifier)))
            .ReturnsAsync(configurationItem);

        // Act
        var actual = await _sut.RegisterProdPipelineAsync(
            _dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _dummyUserEntitlement.User?.MailAddress, _dummyInput);

        // Assert
        actual.ShouldBeEquivalentTo(new BadRequestObjectResult(
            $"The registration failed with a bad request error, " +
            $"because the Configuration Item {configurationItem.CiID} is invalid. " +
            $"Please make sure the CI Environment contains 'Production'"
        ));
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_WithInvalidStatus_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var configurationItem = _fixture.Build<ConfigurationItem>()
            .With(item => item.CiType, "application").With(x => x.Environment, new[] { "Production" }).Create();
        _cmdbClientMock.Setup(cmdbClient => cmdbClient.GetCiAsync(It.Is<string>(s => s == _dummyInput.CiIdentifier)))
            .ReturnsAsync(configurationItem);

        // Act
        var actual = await _sut.RegisterProdPipelineAsync(
            _dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _dummyUserEntitlement.User?.MailAddress, _dummyInput);

        // Assert
        actual.ShouldBeEquivalentTo(new BadRequestObjectResult(
            $"The registration failed with a bad request error, " +
            $"because the Configuration Item {configurationItem.CiID} is invalid. " +
            $"Please make sure the CI Status is 'In Use - ...'"
        ));
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_WithInvalidUserAuthorization_ShouldReturnUnauthorizedObjectResult()
    {
        // Arrange
        var configurationItem = _fixture.Build<ConfigurationItem>()
            .With(x => x.CiType, "application").With(item => item.Environment, new[] { "Production" })
            .With(x => x.Status, "In Use - ").Create();
        _cmdbClientMock.Setup(cmdbClient => cmdbClient.GetCiAsync(It.Is<string>(s => s == _dummyInput.CiIdentifier)))
            .ReturnsAsync(configurationItem);

        // Act
        var actual = await _sut.RegisterProdPipelineAsync(
            _dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _dummyUserEntitlement.User?.MailAddress, _dummyInput);

        // Assert
        actual.ShouldBeEquivalentTo(new UnauthorizedObjectResult(ErrorMessages.RegistrationUpdateUnAuthorized));
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_WithInvalidConfigurationItem_ShouldBeOfTypeBadRequestObjectResult()
    {
        // Arrange
        var input = _fixture.Create<RegistrationRequest>();

        // Act
        var actual = await _sut.RegisterProdPipelineAsync(
            _dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _dummyUserEntitlement.User?.MailAddress, input);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_ExistingProdRegistrationRegisterAsNonProd_ShouldReturnBadRequest()
    {
        // Arrange
        const string organization = "raboweb-test";
        const string pipelineId = "1";
        const string projectId = "2";
        const string pipelineType = ItemTypes.YamlReleasePipeline;

        _pipelineRegistrationRepositoryMock
            .Setup(m => m.GetAsync(It.Is<GetPipelineRegistrationRequest>(r =>
                r.Organization == organization &&
                r.PipelineId == pipelineId &&
                r.PipelineType == pipelineType &&
                r.ProjectId == projectId)))
            .ReturnsAsync(new List<PipelineRegistration> { new() { PartitionKey = PipelineRegistration.Prod } });

        // Act
        var result = await _sut.RegisterNonProdPipelineAsync(organization, projectId, pipelineId, pipelineType, "3");

        // Assert
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        var objectResult = result as BadRequestObjectResult;
        objectResult!.Value.ShouldBe("The registration failed because the specific pipeline is already registered as a production pipeline.");
    }

    [Fact]
    public async Task RegisterProdPipelineAsync_AlreadyRegisteredPipeline_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        const string currentProfileName = nameof(Profiles.MainframeCobol);
        const string newProfileName = nameof(Profiles.Default);

        var input = new RegistrationRequest
        {
            CiIdentifier = _validCiIdentifier,
            Environment = _dummyStageId,
            Profile = newProfileName
        };

        var registrations = _fixture.Build<PipelineRegistration>()
            .With(x => x.PartitionKey, PipelineRegistration.Prod)
            .With(x => x.RuleProfileName, currentProfileName)
            .With(x => x.CiIdentifier, input.CiIdentifier)
            .CreateMany(1).ToList();

        _pipelineRegistrationRepositoryMock.Setup(x => x.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(registrations);

        _pipelineRegistrationStorageMock.Setup(x => x.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()))
            .Verifiable();

        var ci = new ConfigurationItem
        {
            CiID = input.CiIdentifier,
            CiName = _dummyCiName,
            ConfigAdminGroup = _dummyAssignmentGroup,
            CiType = "application",
            Status = "In Use - Production",
            Environment = new List<string> { "Production" },
        };

        var expectedDeploymentMethod = new DeploymentMethod(ci.CiName, _dummyOrganization, _dummyProjectId, _dummyPipelineId, _dummyStageId, currentProfileName);
        var deploymentInformation = _fixture.Build<DeploymentInformation>()
            .With(di => di.Information, expectedDeploymentMethod.ToString())
            .With(di => di.Method, DeploymentInformation.AzureDevOpsMethod)
            .CreateMany(1);

        _cmdbClientMock.Setup(c => c.GetCiAsync(input.CiIdentifier)).ReturnsAsync(ci).Verifiable();
        _cmdbClientMock.Setup(c => c.GetDeploymentMethodAsync(ci.CiName)).ReturnsAsync(deploymentInformation).Verifiable();
        _cmdbClientMock.Setup(c => c.GetAssignmentGroupAsync(_dummyAssignmentGroup)).ReturnsAsync(_validAssignmentGroup);

        // Act
        var actual = await _sut.RegisterProdPipelineAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _validUserEntitlement.User?.MailAddress, input);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task RegisterNonProdPipelineAsync_ShouldReturnDefaultProfile()
    {
        // Arrange
        _pipelineRegistrationRepositoryMock.Setup(x => x.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(new List<PipelineRegistration>());

        _pipelineRegistrationStorageMock.Setup(x => x.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()))
            .Verifiable();

        // Act
        var actual = await _sut.RegisterNonProdPipelineAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _dummyStageId);

        // Assert
        _pipelineRegistrationStorageMock.Verify(pipelineRegistrationStorageRepository => pipelineRegistrationStorageRepository.InsertOrMergeEntityAsync(It.Is<PipelineRegistration>(pipelineRegistration => pipelineRegistration.RuleProfileName == "Default")), Times.Exactly(1));
        actual.ShouldBeOfType(typeof(OkResult));
    }

    [Fact]
    public async Task RegisterNonProdPipelineAsync_WithExistingNonProdRegistration_ShouldReturnOkResult()
    {
        // Arrange
        _pipelineRegistrationRepositoryMock.Setup(x => x.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(new List<PipelineRegistration>());

        _pipelineRegistrationStorageMock.Setup(x => x.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()))
            .Verifiable();

        // Act
        var actual = await _sut.RegisterNonProdPipelineAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _dummyStageId);

        // Assert
        _pipelineRegistrationStorageMock.Verify(x => x.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Exactly(1));
        actual.ShouldBeOfType(typeof(OkResult));
    }

    [Fact]
    public async Task RegisterNonProdPipelineAsync_WithNoRegistrations_ShouldReturnOkResult()
    {
        // Arrange
        _pipelineRegistrationRepositoryMock.Setup(x => x.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(Enumerable.Empty<PipelineRegistration>().ToList());

        _pipelineRegistrationStorageMock.Setup(x => x.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()))
            .Verifiable();

        // Act
        var actual = await _sut.RegisterNonProdPipelineAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _dummyStageId);

        // Assert
        _pipelineRegistrationStorageMock.Verify(x => x.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Once);
        actual.ShouldBeOfType(typeof(OkResult));
    }

    [Fact]
    public async Task UpdateNonProdRegistrationAsync_WithFilledParametersAndStageId_ShouldReturnOkResult()
    {
        // Arrange
        _pipelineRegistrationStorageMock.Setup(x => x.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()))
            .Verifiable();
        _pipelineRegistrationRepositoryMock.Setup(x => x.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(new List<PipelineRegistration>());

        // Act
        var actual = await _sut.UpdateNonProdRegistrationAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _dummyStageId);

        // Assert
        _pipelineRegistrationStorageMock.Verify(x => x.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Exactly(2));
        actual.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public async Task UpdateNonProdRegistrationAsync_WithNoStageId_ShouldReturnOkResult()
    {
        // Arrange
        _pipelineRegistrationStorageMock.Setup(x => x.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()))
            .Verifiable();
        _pipelineRegistrationRepositoryMock.Setup(x => x.GetAsync(It.IsAny<GetPipelineRegistrationRequest>()))
            .ReturnsAsync(new List<PipelineRegistration>());

        // Act
        var actual = await _sut.UpdateNonProdRegistrationAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, null);

        // Assert
        _pipelineRegistrationStorageMock.Verify(x => x.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Once);
        actual.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public async Task UpdateProdPipelineRegistrationAsync_WhenUpdatingCi_ShouldReturnOkResult()
    {
        const string ciName = "CiName";
        const string newCiName = "NewCiName";
        var input = new UpdateRequest
        {
            FieldToUpdate = nameof(FieldToUpdate.CiIdentifier),
            CiIdentifier = "CI1234567",
            NewValue = "CI8912345",
            Environment = _fixture.Create<string>(),
            Profile = _fixture.Create<string>(),
        };

        var ci = new ConfigurationItem
        {
            CiID = input.CiIdentifier,
            CiName = ciName,
            ConfigAdminGroup = _dummyAssignmentGroup,
            CiType = "application",
            Status = "In Use - Production",
            Environment = new List<string> { "Production" },
        };

        var newCi = new ConfigurationItem
        {
            CiID = input.NewValue,
            CiName = newCiName,
            ConfigAdminGroup = _dummyAssignmentGroup,
            CiType = "application",
            Status = "In Use - Production",
            Environment = new List<string> { "Production" },
        };

        var assignment = new AssignmentGroup
        {
            GroupMembers = new List<string>
            {
                "test@raboag.com",
                "upn@rabobank.com"
            }
        };

        var expectedDeploymentMethod = new DeploymentMethod(ci.CiName, _dummyOrganization, _dummyProjectId, _dummyPipelineId, input.Environment, input.FieldToUpdate);
        var deploymentInformation = _fixture.Build<DeploymentInformation>()
            .With(di => di.Information, expectedDeploymentMethod.ToString())
            .With(di => di.Method, DeploymentInformation.AzureDevOpsMethod)
            .CreateMany(1);

        _cmdbClientMock.Setup(c => c.GetCiAsync(input.CiIdentifier)).ReturnsAsync(ci);
        _cmdbClientMock.Setup(c => c.GetCiAsync(input.NewValue)).ReturnsAsync(newCi);
        _cmdbClientMock.Setup(c => c.GetDeploymentMethodAsync(ci.CiName)).ReturnsAsync(deploymentInformation);

        _cmdbClientMock.Setup(c => c.GetAssignmentGroupAsync(_dummyAssignmentGroup)).ReturnsAsync(assignment);
        _pipelineRegistrationRepositoryMock.Setup(x => x.GetAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, input.Environment))
            .ReturnsAsync(new List<PipelineRegistration>()
            {
                new()
                {
                    StageId = input.Environment,
                    CiIdentifier = input.CiIdentifier
                }
            })
            .Verifiable();

        //act
        var actual = await _sut.UpdateProdPipelineRegistrationAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _validUserEmailAddress, input);

        //assert
        _cmdbClientMock.Verify(cmdbClient => cmdbClient.DeleteDeploymentMethodAsync(ci, It.IsAny<SupplementaryInformation>()), Times.Once);
        _cmdbClientMock.Verify(cmdbClient => cmdbClient.InsertDeploymentMethodAsync(It.Is<DeploymentMethod>(deploymentMethod => deploymentMethod.CiName == newCiName)), Times.Once);
        actual.ShouldBeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateProdPipelineRegistrationAsync_WhenUpdatingEnvironment_ShouldReturnOkResult()
    {
        const string ciName = "CiName";
        var input = new UpdateRequest
        {
            FieldToUpdate = nameof(FieldToUpdate.Environment),
            CiIdentifier = "CI1234567",
            NewValue = "Stage2",
            Environment = "Stage1",
            Profile = _fixture.Create<string>(),
        };
        var deploymentInfo = new DeploymentInformation
        {
            Information = $"{{\"organization\":\"{_dummyOrganization}\",\"project\":\"{_dummyProjectId}\",\"pipeline\":\"{_dummyPipelineId}\",\"stage\":\"{input.Environment}\"}}",
            Method = "Azure Devops"
        };

        var ci = new ConfigurationItem
        {
            CiID = input.CiIdentifier,
            CiName = ciName,
            ConfigAdminGroup = _dummyAssignmentGroup,
            CiType = "application",
            Status = "In Use - Production",
            Environment = new List<string> { input.Environment },
        };

        var assignment = new AssignmentGroup
        {
            GroupMembers = new List<string>
            {
                "test@raboag.com",
                "upn@rabobank.com"
            }
        };

        _cmdbClientMock.Setup(c => c.GetCiAsync(input.CiIdentifier)).ReturnsAsync(ci);
        _cmdbClientMock.Setup(c => c.GetAssignmentGroupAsync(_dummyAssignmentGroup)).ReturnsAsync(assignment);
        _cmdbClientMock.Setup(c => c.GetDeploymentMethodAsync(ciName)).ReturnsAsync(new List<DeploymentInformation> { deploymentInfo });
        _pipelineRegistrationRepositoryMock.Setup(x => x.GetAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, input.Environment))
            .ReturnsAsync(new List<PipelineRegistration>()
            {
                new()
                {
                    StageId = input.Environment,
                    CiIdentifier = input.CiIdentifier
                }
            })
            .Verifiable();

        //act
        var actual = await _sut.UpdateProdPipelineRegistrationAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _validUserEmailAddress, input);

        //assert
        _cmdbClientMock.Verify(cmdbClient => cmdbClient.UpdateDeploymentMethodAsync(ci, It.Is<SupplementaryInformation>(supplementaryInformation => supplementaryInformation.Stage == input.Environment), It.Is<DeploymentMethod>(c => c.Stage == input.NewValue)), Times.Once);
        _pipelineRegistrationStorageMock.Verify(m => m.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Once);
        actual.ShouldBeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateProdPipelineRegistrationAsync_WhenUpdatingProfile_ShouldReturnOkResult()
    {
        const string ciName = "CiName";
        var input = new UpdateRequest
        {
            FieldToUpdate = nameof(FieldToUpdate.Profile),
            CiIdentifier = "CI1234567",
            Environment = "Stage1",
            Profile = "Default",
            NewValue = "MainframeCobol",
        };
        var deploymentInfo = new DeploymentInformation
        {
            Information = $"{{\"organization\":\"{_dummyOrganization}\",\"project\":\"{_dummyProjectId}\",\"pipeline\":\"{_dummyPipelineId}\",\"stage\":\"{input.Environment}\", \"profile\":\"{input.Profile}\"}}",
            Method = "Azure Devops"
        };

        var ci = new ConfigurationItem
        {
            CiID = input.CiIdentifier,
            CiName = ciName,
            ConfigAdminGroup = _dummyAssignmentGroup,
            CiType = "application",
            Status = "In Use - Production",
            Environment = new List<string> { input.Environment },
        };

        var assignment = new AssignmentGroup
        {
            GroupMembers = new List<string>
            {
                "test@raboag.com",
                "upn@rabobank.com"
            }
        };

        _cmdbClientMock.Setup(c => c.GetCiAsync(input.CiIdentifier)).ReturnsAsync(ci);
        _cmdbClientMock.Setup(c => c.GetAssignmentGroupAsync(_dummyAssignmentGroup)).ReturnsAsync(assignment);
        _cmdbClientMock.Setup(c => c.GetDeploymentMethodAsync(ciName)).ReturnsAsync(new List<DeploymentInformation> { deploymentInfo });
        _pipelineRegistrationRepositoryMock
            .Setup(m => m.GetAsync(It.Is<GetPipelineRegistrationRequest>(r =>
                r.Organization == _dummyOrganization &&
                r.PipelineId == _dummyPipelineId &&
                r.PipelineType == _validPipelineType &&
                r.ProjectId == _dummyProjectId
            )))
            .ReturnsAsync(new List<PipelineRegistration>
            {
                new()
                {
                    RuleProfileName = input.Profile,
                    CiName = ci.CiName,
                    PartitionKey = "PROD",
                    CiIdentifier = input.CiIdentifier
                }
            })
            .Verifiable();

        //act
        var actual = await _sut.UpdateProdPipelineRegistrationAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId, _validPipelineType, _validUserEmailAddress, input);

        //assert
        _cmdbClientMock.Verify(c => c.UpdateDeploymentMethodAsync(ci, It.IsAny<SupplementaryInformation>(), It.IsAny<DeploymentMethod>()), Times.Once);
        _pipelineRegistrationStorageMock.Verify(m => m.InsertOrMergeEntityAsync(It.IsAny<PipelineRegistration>()), Times.Once);
        actual.ShouldBeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UserIsAuthorized_WhenUserMailAddressIsNull_ShouldReturnFalse()
    {
        // Arrange
        string userMailAddress = null;
        var configurationItem = _fixture.Create<ConfigurationItem>();
        configurationItem.ConfigAdminGroup = null;

        // Act
        // ReSharper disable once ExpressionIsAlwaysNull
        var actual = await _sut.UserIsAuthorizedAsync(userMailAddress, configurationItem);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public async Task UserIsAuthorized_WhenAssignmentGroupContainsUserMailAddress_ShouldReturnTrue()
    {
        // Arrange
        const string userMailAddress = "test@test.com";
        var assignmentGroup = _fixture.Build<AssignmentGroup>().With(x => x.GroupMembers, new[] { userMailAddress })
            .Create();
        var configurationItem = _fixture.Create<ConfigurationItem>();

        _cmdbClientMock.Setup(cmdbClient => cmdbClient.GetAssignmentGroupAsync(
                It.Is<string>(s => s == configurationItem.ConfigAdminGroup)))
            .ReturnsAsync(assignmentGroup);

        // Act
        var actual = await _sut.UserIsAuthorizedAsync(userMailAddress, configurationItem);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public void RegistrationExists_WithNullInformationJson_ShouldReturnFalse()
    {
        // Arrange
        var deploymentInformations = _fixture.Build<DeploymentInformation>()
            .With(x => x.Information, string.Empty).With(x => x.Method, "Azure Devops").CreateMany(1);

        // Act
        var actual = PipelineRegistrator.RegistrationExists(deploymentInformations, null, null, null, null);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public void RegistrationExists_WithDifferentCasingForStageId_ShouldReturnTrue()
    {
        // Arrange
        const string stageId = "Production";

        var deploymentInformations = _fixture.Build<DeploymentInformation>()
            .With(x => x.Information, $"{{\"organization\":\"{_dummyOrganization}\",\"project\":\"{_dummyProjectId}\",\"pipeline\":\"{_dummyPipelineId}\",\"stage\":\"{stageId}\"}}")
            .With(x => x.Method, "Azure Devops")
            .CreateMany(1);

        // Act
        var actual = PipelineRegistrator.RegistrationExists(deploymentInformations, _dummyOrganization, _dummyProjectId, _dummyPipelineId, "production");

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteProdPipelineRegistrationAsync_WithNoExistingConfigurationItem_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var userMailAddress = _fixture.Create<string>();
        var request = _fixture.Create<DeleteRegistrationRequest>();

        // Act
        var actual = (await _sut.DeleteProdPipelineRegistrationAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId,
            _validPipelineType, userMailAddress, request)) as BadRequestObjectResult;

        // Assert
        actual.ShouldNotBeNull();
        actual.Value.ShouldBe(ErrorMessages.CiDoesNotExist(request.CiIdentifier));
    }

    [Fact]
    public async Task DeleteProdPipelineRegistrationAsync_UserNotAuthorized_ShouldReturnUnauthorizedObjectResult()
    {
        // Arrange
        var userMailAddress = _fixture.Create<string>();
        var request = _fixture.Create<DeleteRegistrationRequest>();
        var configurationItem = _fixture.Create<ConfigurationItem>();

        _cmdbClientMock.Setup(x => x.GetCiAsync(It.IsAny<string>())).
            ReturnsAsync(configurationItem);

        // Act
        var actual = (await _sut.DeleteProdPipelineRegistrationAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId,
            _validPipelineType, userMailAddress, request)) as UnauthorizedObjectResult;

        // Assert
        actual.ShouldNotBeNull();
        actual.Value.ShouldBe(ErrorMessages.RegistrationDeleteUnAuthorized);
    }

    [Fact]
    public async Task DeleteProdPipelineRegistrationAsync_UserAuthorized_ShouldReturnOkObjectResult()
    {
        // Arrange
        SetupFixture();

        const string userMailAddress = "test@raboag.com";
        var request = _fixture.Create<DeleteRegistrationRequest>();
        var configurationItem = _fixture.Create<ConfigurationItem>();
        var assignmentGroup = _fixture.Create<AssignmentGroup>();

        var deploymentMethod = new DeploymentMethod(configurationItem.CiName, _dummyOrganization,
            _dummyProjectId, _dummyPipelineId, request.Environment, nameof(Profiles.Default));

        var deploymentInformations = _fixture.Build<DeploymentInformation>()
            .With(information => information.Method, DeploymentInformation.AzureDevOpsMethod)
            .With(information => information.Information, deploymentMethod.ToString()).CreateMany(1);

        _cmdbClientMock.Setup(cmdbClient => cmdbClient.GetCiAsync(It.IsAny<string>())).
            ReturnsAsync(configurationItem);

        _cmdbClientMock.Setup(cmdbClient => cmdbClient.GetAssignmentGroupAsync(It.IsAny<string>()))
            .ReturnsAsync(assignmentGroup);

        _cmdbClientMock.Setup(cmdbClient => cmdbClient.GetDeploymentMethodAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(deploymentInformations));

        _cmdbClientMock.Setup(cmdbClient => cmdbClient.DeleteDeploymentMethodAsync(It.IsAny<ConfigurationItem>(),
                It.IsAny<SupplementaryInformation>()))
            .Verifiable();

        // Act
        var actual = await _sut.DeleteProdPipelineRegistrationAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId,
            _validPipelineType, userMailAddress, request) as OkObjectResult;

        // Assert
        actual.ShouldNotBeNull();
        actual.Value.ShouldBe(Constants.SuccessfullMessageResult);
        _cmdbClientMock.Verify();
    }

    [Fact]
    public async Task DeleteProdPipelineRegistrationAsync_WithNoExistingRegistration_ShouldThrowInvalidOperationException()
    {
        // Arrange
        SetupFixture();

        const string userMailAddress = "test@raboag.com";
        var request = _fixture.Create<DeleteRegistrationRequest>();
        var configurationItem = _fixture.Create<ConfigurationItem>();
        var assignmentGroup = _fixture.Create<AssignmentGroup>();

        var deploymentInformations = _fixture.Build<DeploymentInformation>()
            .With(x => x.Method, DeploymentInformation.AzureDevOpsMethod)
            .With(x => x.Information, string.Empty).CreateMany(1);

        _cmdbClientMock.Setup(x => x.GetCiAsync(It.IsAny<string>())).
            ReturnsAsync(configurationItem);

        _cmdbClientMock.Setup(x => x.GetAssignmentGroupAsync(It.IsAny<string>()))
            .ReturnsAsync(assignmentGroup);

        _cmdbClientMock.Setup(x => x.GetDeploymentMethodAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(deploymentInformations));

        // Act
        var actual = () => _sut.DeleteProdPipelineRegistrationAsync(_dummyOrganization, _dummyProjectId, _dummyPipelineId,
            _validPipelineType, userMailAddress, request);

        // Assert
        await actual.ShouldThrowAsync<InvalidOperationException>();
    }

    private void SetupFixture()
    {
        const string testMailAddress = "test@raboag.com";
        _fixture.Customize<AssignmentGroup>(x =>
            x.With(p => p.GroupMembers, new List<string>
            {
                testMailAddress,
                "upn@rabobank.com"
            }));
        _fixture.Customize<User>(x =>
            x.With(p => p.MailAddress, testMailAddress));
    }
}