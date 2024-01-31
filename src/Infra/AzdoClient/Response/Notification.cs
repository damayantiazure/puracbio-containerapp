using System;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Notification
{
    public DateTime CreatedDate { get; set; }
    public NotificationDetails Details { get; set; }
    public int Id { get; set; }
    public NotificationResult Result { get; set; }
    public string SubscriptionId { get; set; }
}