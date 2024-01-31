using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class OperationResult
{
    [JsonConstructor]
    public OperationResult(bool isSuccess, IEnumerable<KeyValuePair<int, string>> errors, Guid userId, UserEntitlement result)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? Enumerable.Empty<KeyValuePair<int, string>>();
        UserId = userId;
        Result = result;
    }

    public bool IsSuccess { get; set; }

    public IEnumerable<KeyValuePair<int, string>> Errors { get; set; }

    public Guid UserId { get; set; }

    public UserEntitlement Result { get; set; }
}