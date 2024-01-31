#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Infra.StorageClient.Model;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Activities;

public class UploadCompliancyToLogAnalyticsActivity
{
    private readonly ILoggingService _loggingService;

    public UploadCompliancyToLogAnalyticsActivity(ILoggingService loggingService) =>
        _loggingService = loggingService;

    [FunctionName(nameof(UploadCompliancyToLogAnalyticsActivity))]
    public async Task RunAsync([ActivityTrigger] (string organization, string projectId, CompliancyReport report) input)
    {
        var (organization, projectId, report) = input;

        await Task.WhenAll(new List<Task>
        {
            UploadCiCompliancyAsync(organization, projectId, report),
            UploadPrincipleCompliancyAsync(organization, projectId, report),
            UploadRuleCompliancyAsync(organization, projectId, report),
            UploadItemCompliancyAsync(organization, projectId, report),
            UploadPipelineRegistrationsAsync(organization, projectId, report)
        });
    }

    private async Task UploadCiCompliancyAsync(string organization, string projectId, CompliancyReport report)
    {
        var rows = report.RegisteredConfigurationItems?
            .Select(ciReport =>
                new CiReport(ciReport.Id, ciReport.Name, report.Date)
                {
                    Organization = organization,
                    ProjectId = projectId,
                    ProjectName = report.Id,
                    AssignmentGroup = ciReport.AssignmentGroup,
                    IsSOx = ciReport.IsSOx,
                    AicRating = ciReport.AicRating,
                    CiSubtype = ciReport.CiSubtype,
                    PrincipleReports = ciReport.PrincipleReports
                });

        if (rows == null || !rows.Any())
        {
            return;
        }

        await _loggingService.LogInformationItemsAsync(LogDestinations.CompliancyCis, rows);
    }

    private async Task UploadPrincipleCompliancyAsync(string organization, string projectId, CompliancyReport report)
    {
        var rows = report.RegisteredConfigurationItems?
            .Where(ciReport => ciReport.PrincipleReports != null)
            .SelectMany(ciReport => ciReport.PrincipleReports!.Select(principleReport =>
                new PrincipleReport(principleReport.Name, report.Date)
                {
                    Organization = organization,
                    ProjectId = projectId,
                    ProjectName = report.Id,
                    CiId = ciReport.Id,
                    CiName = ciReport.Name,
                    RuleReports = principleReport.RuleReports,
                    HasRulesToCheck = principleReport.HasRulesToCheck,
                    IsSox = principleReport.IsSox
                }));

        if (rows == null || !rows.Any())
        {
            return;
        }

        await _loggingService.LogInformationItemsAsync(LogDestinations.CompliancyPrinciples, rows);
    }

    private async Task UploadRuleCompliancyAsync(string organization, string projectId, CompliancyReport report)
    {
        var rows = report.RegisteredConfigurationItems?
            .Where(ciReport => ciReport.PrincipleReports != null)
            .SelectMany(
                ciReport => ciReport.PrincipleReports!
                    .Where(principleReport => principleReport.RuleReports != null)
                    .SelectMany(principleReport => principleReport.RuleReports!
                        .Select(ruleReport =>
                            new RuleReport(ruleReport.Name, report.Date)
                            {
                                Organization = organization,
                                ProjectId = projectId,
                                ProjectName = report.Id,
                                CiId = ciReport.Id,
                                CiName = ciReport.Name,
                                PrincipleName = principleReport.Name,
                                RuleDocumentation = ruleReport.DocumentationUrl?.AbsoluteUri,
                                ItemReports = ruleReport.ItemReports,
                                Description = ruleReport.Description
                            })));

        if (rows == null || !rows.Any())
        {
            return;
        }

        await _loggingService.LogInformationItemsAsync(LogDestinations.CompliancyRules, rows);
    }

    private async Task UploadItemCompliancyAsync(
        string organization, string projectId, CompliancyReport report)
    {
        var rows = report.RegisteredConfigurationItems?
            .Where(ciReport => ciReport.PrincipleReports != null)
            .SelectMany(ciReport => ciReport.PrincipleReports!
                .Where(principleReport => principleReport.RuleReports != null)
                .SelectMany(principleReport => principleReport.RuleReports!
                    .Where(ruleReport => ruleReport.ItemReports != null)
                    .SelectMany(ruleReport => ruleReport.ItemReports!.Select(itemReport =>
                        new ItemReport(itemReport.ItemId, itemReport.Name, projectId, report.Date)
                        {
                            Deviation = itemReport.Deviation,
                            IsCompliantForRule = itemReport.IsCompliantForRule,
                            CiId = ciReport.Id,
                            CiName = ciReport.Name,
                            Organization = organization,
                            PrincipleName = principleReport.Name,
                            ProjectName = report.Id,
                            RuleName = ruleReport.Name,
                            Type = itemReport.Type
                        }))));

        if (rows == null || !rows.Any())
        {
            return;
        }

        await _loggingService.LogInformationItemsAsync(LogDestinations.CompliancyItems, rows);
    }

    private async Task UploadPipelineRegistrationsAsync(string organization, string projectId, CompliancyReport report)
    {
        var registeredPipelines = report.RegisteredPipelines?
                                      .Select(pipelineReport => ToCompliancyPipelinesRow(organization, projectId,
                                          report, pipelineReport,
                                          pipelineReport.CiNames, pipelineReport.AssignmentGroups,
                                          pipelineReport.IsProduction == true
                                              ? PipelineRegistration.Prod
                                              : PipelineRegistration.NonProd)) ??
                                  Array.Empty<CompliancyPipelineReport>();

        var unregisteredPipelines = report.UnregisteredPipelines?
                                        .Select(pipelineReport => ToCompliancyPipelinesRow(organization, projectId,
                                            report, pipelineReport, null,
                                            null, "Unknown")) ??
                                    Array.Empty<CompliancyPipelineReport>();

        var rows = registeredPipelines.Concat(unregisteredPipelines).ToList();

        if (!rows.Any())
        {
            return;
        }

        await _loggingService.LogInformationItemsAsync(LogDestinations.CompliancyPipelines, rows);
    }

    private static CompliancyPipelineReport ToCompliancyPipelinesRow(string organization, string projectId,
        CompliancyReport compliancyReport, PipelineReport pipelineReport,
        string? ciNames, string? assignmentGroups, string registrationStatus) => new()
        {
            Organization = organization,
            ProjectId = projectId,
            ProjectName = compliancyReport.Id,
            PipelineId = pipelineReport.Id,
            PipelineName = pipelineReport.Name,
            PipelineType = pipelineReport.Type,
            PipelineUrl = pipelineReport.Link,
            ScanDate = compliancyReport.Date,
            CiNames = ciNames,
            AssignmentGroups = assignmentGroups,
            RegistrationStatus = registrationStatus
        };
}