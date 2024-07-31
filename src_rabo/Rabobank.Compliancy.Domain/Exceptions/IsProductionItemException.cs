using System.Runtime.Serialization;

namespace Rabobank.Compliancy.Domain.Exceptions;

[Serializable]
public class IsProductionItemException : Exception
{
    private static string BuildMessage(string date, string ciName, string runUrl) =>
        @$"An error occurred while opening the permissions for this pipeline or repository.
This item has been part of the audit trail for production deployment(s) in the past 450 days.
Therefore, permissions cannot be opened.
The last deployment was on: {date}.
The CI to which this deployment is linked was: {ciName}.
The pipeline run can be found here: {runUrl}.";

    public IsProductionItemException()
    {
    }

    public IsProductionItemException(string date, string ciName, string runUrl) : base(BuildMessage(date, ciName, runUrl))
    {
    }

    public IsProductionItemException(string date, string ciName, string runUrl, Exception innerException) : base(BuildMessage(date, ciName, runUrl), innerException)
    {
    }

    protected IsProductionItemException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}