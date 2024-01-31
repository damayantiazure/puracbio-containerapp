namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public enum ResponseStatus
{
    None,
    Completed,
    Error,
    TimedOut,
    Aborted
}