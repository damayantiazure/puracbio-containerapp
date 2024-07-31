using System;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class GitUserDate
{
    public DateTime Date { get; set; }
    public string Email { get; set; }
    public string ImageUrl { get; set; }
    public string Name { get; set; }
}