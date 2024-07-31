using Rabobank.Compliancy.Application.Requests;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Application.Interfaces;

public interface IRegisterDeviationProcess
{
    /// <summary>
    /// Arranges the whole process for registering deviations
    /// </summary>
    /// <param name="registerDeviationRequest"></param>
    /// <param name="authenticationHeaderValue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RegisterDeviation(RegisterDeviationRequest registerDeviationRequest, AuthenticationHeaderValue authenticationHeaderValue, CancellationToken cancellationToken = default);
}