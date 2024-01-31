using AutoMapper;
using Azure;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDataTablesClient;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Deviations;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Exceptions;
using Rabobank.Compliancy.Clients.AzureQueueClient;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;
using Rabobank.Compliancy.Infrastructure.Dto.Queue;
using Rabobank.Compliancy.Infrastructure.Extensions;
using TableStorage.Abstractions.Store;

namespace Rabobank.Compliancy.Infrastructure.Tests
{
    public class DeviationServiceTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<ITableStore<DeviationEntity>> _tableStorageMock;
        private readonly Mock<IQueueClientFacade> _queueClient;
        private readonly Mock<IMapper> _mapperMock;
        private readonly IDeviationService _sut;

        public DeviationServiceTests()
        {
            _fixture.Customize(new ProjectWithoutPermissions());
            _tableStorageMock = new Mock<ITableStore<DeviationEntity>>();
            _queueClient = new Mock<IQueueClientFacade>();
            _mapperMock = new Mock<IMapper>();
            _sut = new DeviationService(() => _tableStorageMock.Object, _queueClient.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateOrReplaceDeviationAsync_WithValidDeviation_SavesExpectedData()
        {
            // Arrange
            var deviation = _fixture.Create<Deviation>();

            // Act
            await _sut.CreateOrReplaceDeviationAsync(deviation, deviation.UpdatedBy!);

            // Assert
            _tableStorageMock.Verify(repository => repository.InsertOrReplaceAsync(It.Is<DeviationEntity>(d =>
                d.CiIdentifier == deviation.CiIdentifier &&
                d.Comment == deviation.Comment &&
                d.Date < DateTime.UtcNow &&
                d.Date > DateTime.UtcNow.AddHours(-1) &&
                d.ForeignProjectId == deviation.ItemProjectId.ToString() &&
                d.ItemId == deviation.ItemId &&
                d.Organization == deviation.Project.Organization &&
                d.PartitionKey == deviation.Project.Id.ToString() &&
                d.ProjectId == deviation.Project.Id.ToString() &&
                d.ProjectName == deviation.Project.Name &&
                d.Reason == deviation.Reason.ToString() &&
                d.ReasonNotApplicable == deviation.ReasonNotApplicable.ToString() &&
                d.ReasonNotApplicableOther == deviation.ReasonNotApplicableOther &&
                d.ReasonOther == deviation.ReasonOther &&
                d.RowKey == RowKeyGenerator.GenerateRowKey(deviation.Project.Organization, deviation.Project.Id,
                    deviation.RuleName, deviation.ItemId, deviation.CiIdentifier, deviation.ItemProjectId) &&
                d.RuleName == deviation.RuleName &&
                d.UpdatedBy == deviation.UpdatedBy
            ), default), Times.Once);
        }

        [Fact]
        public async Task GetDeviationAsync_WithSuppliedParameters_GeneratesCorrectRowKey()
        {
            // Arrange
            var domainDeviation = _fixture.Create<Deviation>();
            var deviationEntity = domainDeviation.ToEntity(domainDeviation.UpdatedBy!);
            var rowKey = RowKeyGenerator.GenerateRowKey(domainDeviation.Project.Organization, domainDeviation.Project.Id,
                domainDeviation.RuleName, domainDeviation.ItemId, domainDeviation.CiIdentifier,
                domainDeviation.ItemProjectId);

            _tableStorageMock.Setup(repository =>
                    repository.GetRecordAsync(domainDeviation.Project.Id.ToString(),
                        rowKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deviationEntity);

            // Act
            var actual = await _sut.GetDeviationAsync(domainDeviation.Project,
                domainDeviation.RuleName, domainDeviation.ItemId, domainDeviation.CiIdentifier,
                domainDeviation.ItemProjectId);

            // Assert
            actual.Should().BeEquivalentTo(domainDeviation);
        }

        [Fact]
        public async Task GetDeviationAsync_WithNullResult_ReturnsNull()
        {
            // Arrange
            var expectedDeviation = _fixture.Create<Deviation>();

            // Act
            var actual = await _sut.GetDeviationAsync(expectedDeviation.Project,
                expectedDeviation.RuleName, expectedDeviation.ItemId, expectedDeviation.CiIdentifier,
                expectedDeviation.ItemProjectId);

            // Assert
            actual.Should().BeNull();
        }

        [Fact]
        public async Task GetDeviationAsync_WithNotFoundException_ReturnsNull()
        {
            // Arrange
            var expectedDeviation = _fixture.Create<Deviation>();
            _tableStorageMock.Setup(repository =>
                    repository.GetRecordAsync(expectedDeviation.Project.Id.ToString(),
                        It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, string.Empty));

            // Act
            var actual = await _sut.GetDeviationAsync(expectedDeviation.Project,
                expectedDeviation.RuleName, expectedDeviation.ItemId, expectedDeviation.CiIdentifier,
                expectedDeviation.ItemProjectId);

            // Assert
            actual.Should().BeNull();
        }

        [Fact]
        public async Task GetDeviationAsync_WithAnyOtherException_Throws()
        {
            // Arrange
            var expectedDeviation = _fixture.Create<Deviation>();
            _tableStorageMock
                .Setup(repository =>
                    repository.GetRecordAsync(expectedDeviation.Project.Id.ToString(), It.IsAny<string>(), default))
                .ThrowsAsync(new RequestFailedException(400, string.Empty));

            // Act / Assert
            await Assert.ThrowsAsync<RequestFailedException>(() => _sut.GetDeviationAsync(
                expectedDeviation.Project,
                expectedDeviation.RuleName, expectedDeviation.ItemId, expectedDeviation.CiIdentifier,
                expectedDeviation.ItemProjectId));
        }

        [Fact]
        public async Task GetDeviationAsync_WithCorruptedData_ThrowsUnexpectedDataException()
        {
            // Arrange
            var expectedDeviation = _fixture.Create<Deviation>();
            var deviationEntity = expectedDeviation.ToEntity(expectedDeviation.UpdatedBy!);
            var rowKey = RowKeyGenerator.GenerateRowKey(expectedDeviation.Project.Organization,
                expectedDeviation.Project.Id,
                expectedDeviation.RuleName, expectedDeviation.ItemId, expectedDeviation.CiIdentifier,
                expectedDeviation.ItemProjectId);
            _tableStorageMock
                .Setup(repository => repository.GetRecordAsync(expectedDeviation.Project.Id.ToString(), rowKey, default))
                .ReturnsAsync(() => deviationEntity);

            // Act + Assert
            deviationEntity.Reason = null;
            var reason = deviationEntity.Reason;
            await Assert.ThrowsAsync<UnexpectedDataException>(() => _sut.GetDeviationAsync(
                expectedDeviation.Project,
                expectedDeviation.RuleName, expectedDeviation.ItemId, expectedDeviation.CiIdentifier,
                expectedDeviation.ItemProjectId));


            deviationEntity.Reason = reason;
            deviationEntity.ProjectId = null;
            await Assert.ThrowsAsync<UnexpectedDataException>(() => _sut.GetDeviationAsync(
                expectedDeviation.Project,
                expectedDeviation.RuleName, expectedDeviation.ItemId, expectedDeviation.CiIdentifier,
                expectedDeviation.ItemProjectId));
        }

        [Fact]
        public async Task GetDeviationsAsync_WithProjectId_ShouldReturnDeviations()
        {
            // Arrange
            var projectId = _fixture.Create<Guid>();

            var deviationEntities = _fixture.Build<DeviationEntity>()
                .With(f => f.ProjectId, projectId.ToString())
                .With(f => f.ForeignProjectId, Guid.NewGuid().ToString())
                .With(f => f.Reason, _fixture.Create<DeviationReason>().ToString)
                .With(f => f.ReasonNotApplicable, _fixture.Create<DeviationApplicabilityReason>().ToString)
                .CreateMany(1);

            _tableStorageMock
                .Setup(repository => repository.GetByPartitionKeyAsync(projectId.ToString(), default))
                .ReturnsAsync(() => deviationEntities);

            // Act
            var actual = (await _sut.GetDeviationsAsync(projectId)).ToList();

            // Assert
            actual.Single().Project.Id.Should().Be(projectId);
        }

        [Fact]
        public async Task GetDeviationsAsync_NoneFound_ShouldReturnNull()
        {
            // Arrange
            var projectId = _fixture.Create<Guid>();

            _tableStorageMock
                .Setup(repository => repository.GetByPartitionKeyAsync(projectId.ToString(), default))
                .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, string.Empty));

            // Act
            var actual = await _sut.GetDeviationsAsync(projectId);

            // Assert
            actual.Should().HaveCount(0);
        }

        [Fact]
        public async Task GetDeviationsAsync_OtherError_ShouldThrow()
        {
            // Arrange
            var projectId = _fixture.Create<Guid>();

            _tableStorageMock
                .Setup(repository => repository.GetByPartitionKeyAsync(projectId.ToString(), default))
                .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.InternalServerError, string.Empty));

            // Act
            var func = () => _sut.GetDeviationsAsync(projectId);

            // Assert
            await func.Should().ThrowAsync<RequestFailedException>();
        }

        [Fact]
        public async Task DeleteDeviationAsync_WithCorrectlyFilledDeviation_ShouldDeleteTheDeviation()
        {
            // Arrange
            var deviation = _fixture.Create<Deviation>();
            _tableStorageMock.Setup(x => x.DeleteAsync(It.IsAny<DeviationEntity>(), default));

            // Act
            await _sut.DeleteDeviationAsync(deviation);

            // Assert
            _tableStorageMock.Verify(x => x.DeleteAsync(It.IsAny<DeviationEntity>(), default), Times.Once);
        }

        [Theory]
        [InlineData(DeviationReportLogRecordType.Insert)]
        [InlineData(DeviationReportLogRecordType.Delete)]
        public async Task SendDeviationUpdateRecord_Deviation_RecordSentToQueueWithCorrectRecordType(DeviationReportLogRecordType recordType)
        {
            // Arrange
            var deviation = _fixture.Create<Deviation>();
            var deviationLogEntity = _fixture.Create<DeviationQueueDto>();
            deviationLogEntity.RecordType = recordType.ToString();
            _mapperMock
                .Setup(m => m.Map<DeviationQueueDto>(It.IsAny<Deviation>()))
                .Returns(deviationLogEntity);

            // Act
            await _sut.SendDeviationUpdateRecord(deviation, recordType);

            // Assert
            _queueClient.Verify(x => x.SendMessageAsync(deviationLogEntity));
        }
    }
}