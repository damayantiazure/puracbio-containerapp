using Rabobank.Compliancy.Domain.RuleProfiles;

namespace Rabobank.Compliancy.Domain.Compliancy.Registrations;

public abstract class PipelineRegistration
{
    public Pipeline Pipeline { get; set; }

    public RuleProfile RuleProfile { get; set; }
}