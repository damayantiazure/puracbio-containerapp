namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Entitlements<T>
{
    public string ContinuationToken { get; set; }

    public T[] Items { get; set; }
}