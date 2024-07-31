namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;

/// <summary>
/// Object that contains a collection of response objects the way Azure Devops returns it.
/// </summary>
/// <typeparam name="TClass"></typeparam>
public class ResponseCollection<TClass> where TClass : class
{
    /// <summary>
    /// The number of response objects contained in the <see cref="Value"/> Value property
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// The collection of response objects.
    /// </summary>
    public IEnumerable<TClass>? Value { get; set; }
}