using System.Text.RegularExpressions;
using AutoMapper;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;
using Rabobank.Compliancy.Infrastructure.Mapping;

namespace Rabobank.Compliancy.Infrastructure.Tests.Mapping;

public class CompliancyReportMappingProfileTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ShouldCorrectlyMapReportToDto()
    {
        // Arrange
        var compliancyReport = _fixture.Create<CompliancyReport>();

        var sut = CreateMapper();

        // Act
        var actual = sut.Map<CompliancyReportDto>(compliancyReport);

        // Assert
        actual.Should().BeEquivalentTo(compliancyReport, options => options

            // ItemReports
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(RuleReport.ItemReports)}\\[\\d\\].{nameof(ItemReport.ScanDate)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(RuleReport.ItemReports)}\\[\\d\\].{nameof(ItemReport.Organization)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(RuleReport.ItemReports)}\\[\\d\\].{nameof(ItemReport.CiId)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(RuleReport.ItemReports)}\\[\\d\\].{nameof(ItemReport.CiName)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(RuleReport.ItemReports)}\\[\\d\\].{nameof(ItemReport.PrincipleName)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(RuleReport.ItemReports)}\\[\\d\\].{nameof(ItemReport.ProjectName)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(RuleReport.ItemReports)}\\[\\d\\].{nameof(ItemReport.RuleName)}"))

            // RuleReports
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(PrincipleReport.RuleReports)}\\[\\d\\].{nameof(RuleReport.ScanDate)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(PrincipleReport.RuleReports)}\\[\\d\\].{nameof(RuleReport.Organization)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(PrincipleReport.RuleReports)}\\[\\d\\].{nameof(RuleReport.ProjectId)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(PrincipleReport.RuleReports)}\\[\\d\\].{nameof(RuleReport.ProjectName)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(PrincipleReport.RuleReports)}\\[\\d\\].{nameof(RuleReport.CiId)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(PrincipleReport.RuleReports)}\\[\\d\\].{nameof(RuleReport.CiName)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(PrincipleReport.RuleReports)}\\[\\d\\].{nameof(RuleReport.PrincipleName)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(PrincipleReport.RuleReports)}\\[\\d\\].{nameof(RuleReport.RuleDocumentation)}"))

            // PrincipleReports
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(CiReport.PrincipleReports)}\\[.\\].{nameof(PrincipleReport.ScanDate)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(CiReport.PrincipleReports)}\\[.\\].{nameof(PrincipleReport.ProjectName)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(CiReport.PrincipleReports)}\\[.\\].{nameof(PrincipleReport.CiId)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(CiReport.PrincipleReports)}\\[.\\].{nameof(PrincipleReport.CiName)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(CiReport.PrincipleReports)}\\[.\\].{nameof(PrincipleReport.Organization)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(CiReport.PrincipleReports)}\\[.\\].{nameof(PrincipleReport.ProjectId)}"))

            // RegisteredConfigurationItems
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(CompliancyReport.RegisteredConfigurationItems)}\\[.\\].{nameof(CiReport.ScanDate)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(CompliancyReport.RegisteredConfigurationItems)}\\[.\\].{nameof(CiReport.Organization)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(CompliancyReport.RegisteredConfigurationItems)}\\[.\\].{nameof(CiReport.ProjectId)}"))
            .Excluding(info => Regex.IsMatch(info.Path,
                $"{nameof(CompliancyReport.RegisteredConfigurationItems)}\\[.\\].{nameof(CiReport.ProjectName)}")));
    }

    [Fact]
    public void ShouldCorrectlyMapDtoToReport()
    {
        // Arrange
        var sut = CreateMapper();

        var compliancyReportDto = sut.Map<CompliancyReportDto>(_fixture.Create<CompliancyReport>());

        // Act
        var actual = sut.Map<CompliancyReport>(compliancyReportDto);

        // Assert
        actual.Should().BeEquivalentTo(compliancyReportDto, options => options.ExcludingMissingMembers());
    }

    private static IMapper CreateMapper() =>
        new Mapper(new MapperConfiguration(cfg => { cfg.AddProfile<CompliancyReportMappingProfile>(); }));
}