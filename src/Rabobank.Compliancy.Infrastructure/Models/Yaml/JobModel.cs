namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public class JobModel
{
    public string Job { internal get; set; }

    public string Deployment { internal get; set; }

    public string DisplayName { get; set; }

    public IEnumerable<StepModel> Steps { get; set; }

    public Environment Environment { get; set; }

    public Strategy Strategy { get; set; }

    public string JobId { get { return Deployment ?? Job; } }

    public IEnumerable<StepModel> GetAllSteps()
    {
        if (Strategy == null)
        {
            return Steps;
        }
        if (Strategy.Canary == null && Strategy.RunOnce == null && Strategy.Rolling == null)
        {
            return Steps;
        }

        var returnList = new List<StepModel>();

        if (Strategy.RunOnce != null)
        {
            returnList.AddRange(Strategy.RunOnce.GetAllSteps());
        }
        if (Strategy.Rolling != null)
        {
            returnList.AddRange(Strategy.Rolling.GetAllSteps());
        }
        if (Strategy.Canary != null)
        {
            returnList.AddRange(Strategy.Canary.GetAllSteps());
        }

        return returnList;
    }
}