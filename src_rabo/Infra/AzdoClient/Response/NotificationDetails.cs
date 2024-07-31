using System;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class NotificationDetails
{
    public DateTime CompletedDate { get; set; }
    public string ErrorDetail { get; set; }
    public string ErrorMessage { get; set; }
    public Event Event { get; set; }
    public string EventType { get; set; }
    public PublisherInputs PublisherInputs { get; set; }

}