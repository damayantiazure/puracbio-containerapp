using System;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

public class Reconcile
{
    public Uri Url { get; set; }
    public string[] Impact { get; set; }
}