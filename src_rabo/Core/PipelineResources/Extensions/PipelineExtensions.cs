using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Core.PipelineResources.Extensions;

public static class PipelineExtensions
{
    public static Dictionary<string, string> ToInputsDictionary(this JToken pipelineInputs)
    {
        // Check for JObject otherwise cast will produce an error.
        if (pipelineInputs is not JObject)
        {
            return new Dictionary<string, string>();
        }

        var inputs = new Dictionary<string, string>();
        foreach (var (key, value) in (JObject)pipelineInputs)
        {
            inputs.Add(key, value.ToString());
        }
        return inputs;
    }
}