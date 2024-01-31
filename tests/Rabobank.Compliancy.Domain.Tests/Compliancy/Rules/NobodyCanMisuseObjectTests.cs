using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using Rabobank.Compliancy.Domain.Compliancy.Rules;
using Rabobank.Compliancy.Domain.Tests.Compliancy.Rules.TestImplementations;
using Rabobank.Compliancy.Tests;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class NobodyCanMisuseObjectTests : UnitTestBase
{
    private readonly NobodyCanMisuseObject<TestMisUse> _sut;

    public NobodyCanMisuseObjectTests()
    {
        _sut = new NobodyCanMisuseObjectTestImplementation();
    }

    [Fact]
    public void Evaluate_WhenEvaluatableHasNoPermissionsOnMisuse_ShouldReturnTrueResult()
    {
        // Arrange
        var evaluatable = new MisUsableEvaluatableTestImplementation();

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenEvaluatableHasPermissionsOnMisuse_ShouldReturnFalseResult()
    {
        // Arrange
        var evaluatable = new MisUsableEvaluatableTestImplementation();
        evaluatable.Permissions.Add(TestMisUse.Test, new[] { new Domain.Compliancy.Authorizations.User(InvariantUnitTestValue, InvariantUnitTestValue) });

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WhenEvaluatableHasPermissionsOnMisuseInASubIdentity_ShouldReturnFalseResult()
    {
        // Arrange
        var evaluatable = new MisUsableEvaluatableTestImplementation();
        var group = new Group(InvariantUnitTestValue, InvariantUnitTestValue);
        group.AddMember(new Domain.Compliancy.Authorizations.User(InvariantUnitTestValue, InvariantUnitTestValue));
        evaluatable.Permissions.Add(TestMisUse.Test, new[] { group });

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WhenEvaluatableHasPermissionsOnMisuseInFirstCategory_ShouldReturnFalseResult()
    {
        // Arrange
        var evaluatable = new MisUsableEvaluatableTestImplementation();
        evaluatable.Permissions.Add(TestMisUse.Test, new[] { new Domain.Compliancy.Authorizations.User(InvariantUnitTestValue, InvariantUnitTestValue) });
        evaluatable.Permissions.Add(TestMisUse.Test2, Enumerable.Empty<IIdentity>()); // Would normally come later in the evaluation and set the evaluation to true, this shouldn't happen if the first one was false.

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }
}