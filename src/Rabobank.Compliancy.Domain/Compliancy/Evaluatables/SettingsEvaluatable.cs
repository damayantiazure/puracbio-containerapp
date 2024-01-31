namespace Rabobank.Compliancy.Domain.Compliancy.Evaluatables;

public class SettingsEvaluatable : IEvaluatable
{
    private readonly IEnumerable<ISettings> _settings;

    public SettingsEvaluatable(Pipeline pipeline)
    {
        _settings = pipeline.Settings;
    }

    public T GetSettings<T>() where T : ISettings
    {
        return _settings.OfType<T>().SingleOrDefault();
    }
}