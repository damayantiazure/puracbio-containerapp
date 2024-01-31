using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Rabobank.Compliancy.Infra.AzdoClient.Extensions;

public static class YamlExtensions
{
    public static JToken ToJson(this string yamlText)
    {
        try
        {
            if (yamlText == null)
            {
                return new JObject();
            }

            var deserializer = new DeserializerBuilder().Build();
            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();

            var escapedCharacters = EscapeEncodedNonAsciiCharacters(yamlText);
            var escapedString = EscapeNonPrintableCharacters(escapedCharacters);
            var json = serializer.Serialize(deserializer.Deserialize(new StringReader(escapedString)));

            return JsonConvert.DeserializeObject<JToken>(json);
        }
        catch (Exception ex) when (ex is SyntaxErrorException || ex is InvalidCastException || ex is YamlException)
        {
            return new JObject();
        }
    }

    private static string EscapeEncodedNonAsciiCharacters(string inputString) =>
        Regex.Replace(
            inputString,
            @"[\\]{1,2}[uU]{1}(?<value>[a-fA-F0-9]{4,8})",
            m =>
            {
                return @"\\U" + m.Groups["value"].Value;
            }, RegexOptions.IgnoreCase);

    private static string EscapeNonPrintableCharacters(string inputString) =>
        Regex.Replace(
            inputString,
            @"(?<!\\)(\\e)",
            m =>
            {
                return @"\\e" + m.Groups["value"].Value;
            }, RegexOptions.IgnoreCase);
}