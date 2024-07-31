using AutoFixture;
using Moq;
using Rabobank.Compliancy.Core.Rules.Tests.Implementations;
using Rabobank.Compliancy.Infra.AzdoClient;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Tests.Model;

public class ReconcilableClassicReleasePipelineRuleTests
{
    private readonly Mock<IAzdoRestClient> _azdoClientMock = new();
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public async Task ReconcileAndEvaluateAsyncResult_Equals_EvaluateAsyncResult()
    {
        // Arrange
        var evaluationResult = _fixture.Create<bool>();
        var reconcilableRule = new ReconcilableClassicReleasePipelineRuleImplementation(_azdoClientMock.Object, evaluationResult);

        // Act
        var result = await reconcilableRule.ReconcileAndEvaluateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

        // Assert
        Assert.Equal(evaluationResult, result);
    }
}