using Rabobank.Compliancy.Application.Requests;

namespace Rabobank.Compliancy.Application.Tests.RequestImplementations;

internal class RuleRescanRequestBaseTestImplementation : RuleRescanRequestBase
{
    private readonly string _itemIdAsString;
    public RuleRescanRequestBaseTestImplementation(string organization, Guid reportProjectId, Guid itemProjectId, string ruleName, string itemIdAsString, bool concernsProjectRule)
        : base(organization, reportProjectId, itemProjectId, ruleName, concernsProjectRule)
    {
        _itemIdAsString = itemIdAsString;
    }

    protected override string GetItemIdAsString()
    {
        return _itemIdAsString;
    }
}