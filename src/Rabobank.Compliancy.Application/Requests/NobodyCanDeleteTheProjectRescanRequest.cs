using Rabobank.Compliancy.Domain.Rules;

namespace Rabobank.Compliancy.Application.Requests;

public class NobodyCanDeleteTheProjectRescanRequest : RuleRescanRequestBase
{
    /// <inheritdoc/>
    public NobodyCanDeleteTheProjectRescanRequest(string organization, Guid reportProjectId, Guid scannableProjectId)
        : base(organization, reportProjectId, scannableProjectId, RuleNames.NobodyCanDeleteTheProject, true)
    {
    }

    protected override string GetItemIdAsString()
    {
        return ItemProjectId.ToString();
    }
}