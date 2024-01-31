using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Flurl;

namespace Rabobank.Compliancy.Infra.AzdoClient;

public interface IAzdoRequest
{
    Uri BaseUri(string organization);
    string Resource { get; }
    IDictionary<string, object> QueryParams { get; }
    IDictionary<string, object> Headers { get; }
    Url Url(string organization);
    Url Url();
    int? TimeoutInSeconds { get; }
}

[SuppressMessage("Sonar Code Smell",
    "S2326: TInput, TResponse is not used in the interface.",
    Justification = "IAzdoRequest will be phased out, the benefits doe not justify the effort.")]
public interface IAzdoRequest<TInput, TResponse> : IAzdoRequest
{
}

public interface IAzdoRequest<TResponse> : IAzdoRequest<TResponse, TResponse>
{
}