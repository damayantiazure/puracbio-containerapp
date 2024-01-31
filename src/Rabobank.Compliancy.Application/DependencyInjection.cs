#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Application.Deviations;
using Rabobank.Compliancy.Application.ExclusionList;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Interfaces.Deviations;
using Rabobank.Compliancy.Application.Interfaces.MonitoringDashboard;
using Rabobank.Compliancy.Application.Interfaces.Reconcile;
using Rabobank.Compliancy.Application.ItemRescan;
using Rabobank.Compliancy.Application.MonitoringDashboard;
using Rabobank.Compliancy.Application.Reconcile;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Core.Rules.Processors;

namespace Rabobank.Compliancy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services) =>
        services
            .AddTransient<IBuildPipelineRuleRescanProcess, BuildPipelineRuleRescanProcess>()
            .AddTransient<ICheckAuthorizationProcess, CheckAuthorizationProcess>()
            .AddTransient<IClassicReleasePipelineRuleRescanProcess, ClassicReleasePipelineRuleRescanProcess>()
            .AddTransient<IDeleteDeviationProcess, DeleteDeviationProcess>()
            .AddTransient<IExclusionListProcess, ExclusionListProcess>()
            .AddTransient<IItemReconcileProcess, ItemReconcileProcess>()
            .AddTransient<ILogDeviationRecordProcess, LogDeviationRecordProcess>()
            .AddTransient<IProjectReconcileProcess, ProjectReconcileProcess>()
            .AddTransient<IProjectRuleRescanProcess, ProjectRuleRescanProcess>()
            .AddTransient<IReconcileProcessor, ReconcileProcessor>()
            .AddTransient<IReconcileProcess, ReconcileProcess>()
            .AddTransient<IRegisterDeviationProcess, RegisterDeviationProcess>()
            .AddTransient<IRepositoryRuleRescanProcess, RepositoryRuleRescanProcess>();

    public static IServiceCollection AddMonitoringDashboardTileProcessDependencies(this IServiceCollection services) =>
        services
            .AddTransient<IMonitoringDashboardTileProcess, MonitoringDashboardTileProcess>();
}