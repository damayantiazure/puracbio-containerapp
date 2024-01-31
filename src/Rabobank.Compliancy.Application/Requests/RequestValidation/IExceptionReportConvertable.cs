#nullable enable

using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Application.Requests.RequestValidation;

public interface IExceptionReportConvertible
{
    public ExceptionReport ToExceptionReport(string functionName, string functionUrl, Exception exception);
}