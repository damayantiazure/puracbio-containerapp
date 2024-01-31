using AutoFixture.Kernel;
using AutoMapper;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infrastructure.Dto.Logging;
using Rabobank.Compliancy.Infrastructure.Dto.Queue;
using Rabobank.Compliancy.Infrastructure.Mapping;

namespace Rabobank.Compliancy.Infrastructure.Tests.Mapping;

public class LoggingMappingProfileTests
{
    private readonly Fixture _fixture = new();

    [Theory]
    [InlineData(
        typeof(AuditLoggingReport),
        typeof(AuditDeploymentLogDto),
        new[]
        {
            nameof(AuditLoggingReport.IsSox),
            nameof(AuditLoggingReport.CreatedDate)
        })]
    [InlineData(
        typeof(ExceptionReport),
        typeof(AuditLoggingErrorLogDto), new[]
        {
            nameof(ExceptionReport.Request),
            nameof(ExceptionReport.RequestUrl),
            nameof(ExceptionReport.ItemId),
            nameof(ExceptionReport.ItemType),
            nameof(ExceptionReport.RuleName),
            nameof(ExceptionReport.CiIdentifier),
            nameof(ExceptionReport.UserId),
            nameof(ExceptionReport.UserMail),
            nameof(ExceptionReport.RunId),
            nameof(ExceptionReport.ReleaseId),
            nameof(ExceptionReport.PipelineType),
            nameof(ExceptionReport.PullRequestUrl),
            nameof(ExceptionReport.StageId),
            nameof(ExceptionReport.RequestData)
        })]
    [InlineData(
        typeof(HookFailureReport),
        typeof(AuditLoggingHookFailureLogDto),
        new[]
        { 
            nameof(HookFailureReport.Date)
        })]
    [InlineData(
        typeof(PoisonMessageReport),
        typeof(AuditPoisonMessagesLogDto),
        new[] { nameof(HookFailureReport.Date) })]
    [InlineData(
        typeof(PoisonMessageReport),
        typeof(AuditPoisonMessagesLogDto))]
    [InlineData(
        typeof(AuditLoggingPullRequestReport),
        typeof(AuditPullRequestApproversLogDto))]
    [InlineData(
        typeof(ExceptionReport),
        typeof(ComplianceScannerOnlineErrorLogDto),
        new[]
        {
            nameof(ExceptionReport.RequestData),
            nameof(ExceptionReport.RunId),
            nameof(ExceptionReport.RunUrl),
            nameof(ExceptionReport.ReleaseId),
            nameof(ExceptionReport.ReleaseUrl),
            nameof(ExceptionReport.PipelineType),
            nameof(ExceptionReport.PullRequestUrl),
            nameof(ExceptionReport.UserId)
        })]
    [InlineData(
        typeof(CiReport),
        typeof(CompliancyCisDto),
        new[]
        {
            nameof(CiReport.PrincipleReports),
            nameof(CiReport.RescanUrl),
            nameof(CiReport.IsScanFailed),
            nameof(CiReport.ScanException)
        })]
    [InlineData(
        typeof(ItemReport),
        typeof(CompliancyItemsDto),
        new[]
        {
            nameof(ItemReport.Type),
            nameof(ItemReport.Link),
            nameof(ItemReport.IsCompliantForRule),
            nameof(ItemReport.ReconcileUrl),
            nameof(ItemReport.RescanUrl),
            nameof(ItemReport.RegisterDeviationUrl),
            nameof(ItemReport.DeleteDeviationUrl),
            nameof(ItemReport.ReconcileImpact),
            nameof(ItemReport.Deviation)
        })]
    [InlineData(
        typeof(CompliancyPipelineReport),
        typeof(CompliancyPipelinesDto))]
    [InlineData(
        typeof(PrincipleReport),
        typeof(CompliancyPrinciplesDto),
        new[]
        {
            nameof(PrincipleReport.HasRulesToCheck),
            nameof(PrincipleReport.IsSox),
            nameof(PrincipleReport.RuleReports)
        })]
    [InlineData(
        typeof(RuleReport),
        typeof(CompliancyRulesDto),
        new[]
        {
            nameof(RuleReport.Description),
            nameof(RuleReport.DocumentationUrl),
            nameof(RuleReport.ItemReports)
        })]
    [InlineData(
        typeof(DecoratorErrorReport),
        typeof(DecoratorErrorLogDto))]
    [InlineData(
        typeof(DeviationQueueDto),
        typeof(DeviationsLogDto))]
    [InlineData(
        typeof(ExceptionReport),
        typeof(ErrorHandlingLogDto),
        new[]
        {
            nameof(ExceptionReport.Request),
            nameof(ExceptionReport.RequestUrl),
            nameof(ExceptionReport.RequestData),
            nameof(ExceptionReport.ItemId),
            nameof(ExceptionReport.ItemType),
            nameof(ExceptionReport.RuleName),
            nameof(ExceptionReport.CiIdentifier),
            nameof(ExceptionReport.UserId),
            nameof(ExceptionReport.UserMail),
            nameof(ExceptionReport.RunId),
            nameof(ExceptionReport.RunUrl),
            nameof(ExceptionReport.ReleaseId),
            nameof(ExceptionReport.ReleaseUrl),
            nameof(ExceptionReport.PipelineType),
            nameof(ExceptionReport.PullRequestUrl),
            nameof(ExceptionReport.StageId)
        })]
    [InlineData(
        typeof(ExceptionReport),
        typeof(Sm9ChangesErrorLogDto),
        new[]
        {
            nameof(ExceptionReport.RequestData),
            nameof(ExceptionReport.ItemId),
            nameof(ExceptionReport.ItemType),
            nameof(ExceptionReport.RuleName),
            nameof(ExceptionReport.CiIdentifier),
            nameof(ExceptionReport.UserId),
            nameof(ExceptionReport.UserMail),
            nameof(ExceptionReport.RunUrl),
            nameof(ExceptionReport.ReleaseId),
            nameof(ExceptionReport.ReleaseUrl),
            nameof(ExceptionReport.PullRequestUrl),
            nameof(ExceptionReport.StageId)
        })]
    public void Map_Dto_ShouldBeEquivalentTo_Model(
        Type sourceType, Type destinationType, string[]? excluding = null)
    {
        // Arrange
        var source = _fixture.Create(sourceType, new SpecimenContext(_fixture));

        var sut = new Mapper(new MapperConfiguration(c =>
            c.AddProfile(new LoggingMappingProfile())));

        // Act
        var actual = sut.Map(source, sourceType, destinationType);

        // Assert
        actual.Should().BeEquivalentTo(source, options =>
            options
                .Using<object>(ctx => ctx.Subject.Should().Be(ctx.Expectation.ToString()))
                .When(objectInfo => objectInfo.CompileTimeType == typeof(Uri))
                .Excluding(memberInfo => excluding != null && excluding.Contains(memberInfo.Name)));
    }
}