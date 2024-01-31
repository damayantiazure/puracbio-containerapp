namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public abstract class StrategyImplementation
{
    public PreDeploy PreDeploy { get; set; }
    public Deploy Deploy { get; set; }
    public RouteTraffic RouteTraffic { get; set; }
    public PostRouteTraffic PostRouteTraffic { get; set; }
    public On On { get; set; }
    public IEnumerable<StepModel> GetAllSteps()
    {
        foreach (var step in PreDeploy?.Steps ?? Enumerable.Empty<StepModel>())
        {
            yield return step;
        }
        foreach (var step in Deploy?.Steps ?? Enumerable.Empty<StepModel>())
        {
            yield return step;
        }
        foreach (var step in RouteTraffic?.Steps ?? Enumerable.Empty<StepModel>())
        {
            yield return step;
        }
        foreach (var step in PostRouteTraffic?.Steps ?? Enumerable.Empty<StepModel>())
        {
            yield return step;
        }
        foreach (var step in On?.Success?.Steps ?? Enumerable.Empty<StepModel>())
        {
            yield return step;
        }
        foreach (var step in On?.Failure?.Steps ?? Enumerable.Empty<StepModel>())
        {
            yield return step;
        }
    }
}