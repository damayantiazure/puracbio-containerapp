using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules.TestImplementations;

public class NobodyCanMisuseObjectTestImplementation : NobodyCanMisuseObject<TestMisUse>
{
    protected override IEnumerable<TestMisUse> MisUseTypes => new[] { TestMisUse.Test };
}