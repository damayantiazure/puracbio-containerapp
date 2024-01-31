namespace Rabobank.Compliancy.Core.Rules.Model;

public interface IRule
{
    string Name { get; }
    string Description { get; }
    string Link { get; }
    BluePrintPrinciple[] Principles { get; }
}