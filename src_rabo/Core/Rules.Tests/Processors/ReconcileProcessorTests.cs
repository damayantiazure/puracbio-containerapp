using Moq;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Processors;

namespace Rabobank.Compliancy.Core.Rules.Tests.Processors;

public class ReconcileProcessorTests
{
    private readonly Mock<IReconcile> _itemreconcileRulesMock = new();
    private readonly Mock<IProjectReconcile> _projectReconcileRulesMock = new();
    private readonly ReconcileProcessor _sut;

    public ReconcileProcessorTests()
    {
        _sut = new ReconcileProcessor(new[] { _itemreconcileRulesMock.Object }, new[] { _projectReconcileRulesMock.Object });
    }

    [Fact]
    public void GetAllProjectReconcile()
    {
        // Arrgan & Act
        var actual = _sut.GetAllProjectReconcile();

        // Assert
        actual.ShouldBeEquivalentTo(new[] { _projectReconcileRulesMock.Object });
    }

    [Fact]
    public void GetAllItemReconcile()
    {
        // Arrgan & Act
        var actual = _sut.GetAllItemReconcile();

        // Assert
        actual.ShouldBeEquivalentTo(new[] { _itemreconcileRulesMock.Object });
    }
}