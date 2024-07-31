namespace Rabobank.Compliancy.Functions.Sm9Changes.Application;

public class CreateChangeDetails
{
    public string PriorityTemplate { get; set; }
    public string[] ImplementationPlan { get; set; }
    public string[] Assets { get; set; }
    public int PlannedTime { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}