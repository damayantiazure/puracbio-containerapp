namespace Rabobank.Compliancy.Domain.Compliancy;

/// <summary>
///     This class represents a stage in a PipelineBody.
///     A stage can contain Gates that protect it from running.
/// </summary>
public sealed class Stage : IEquatable<Stage>
{
    public string Id { get; set; }

    public string Name { get; set; }

    public IEnumerable<Gate> Gates { get; set; }

    public bool Equals(Stage other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object obj) => ReferenceEquals(this, obj) || (obj is Stage other && Equals(other));

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Id);

    public static bool operator ==(Stage left, Stage right) => Equals(left, right);

    public static bool operator !=(Stage left, Stage right) => !Equals(left, right);
}