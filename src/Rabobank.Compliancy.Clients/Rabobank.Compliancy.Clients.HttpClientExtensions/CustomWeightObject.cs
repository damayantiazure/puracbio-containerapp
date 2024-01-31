namespace Rabobank.Compliancy.Clients.HttpClientExtensions;

internal class RequestWeightAvarage
{
    public int NumberOfTimesCalled { get; set; }
    public long AvarageRuntimePerRequest { get; set; }
}