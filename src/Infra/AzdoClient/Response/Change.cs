namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Change
{
    public ChangeAuthor Author { get; set; }
    public string Id { get; set; }
    public string Location { get; set; }
    public string Message { get; set; }
    public bool MessageTruncated { get; set; }
    public string Pusher { get; set; }
    public string Timestamp{ get; set; }
    public string Type { get; set; }
}