#nullable enable

using Azure.Monitor.Query.Models;
using Newtonsoft.Json;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Reflection;

namespace Rabobank.Compliancy.Infrastructure.Extensions;

/// <summary>
///     An generic extension class that is used to convert <see cref="WorkspaceResponse" /> response into a domain class.
/// </summary>
internal static class MonitoringExtensions
{
    /// <summary>
    ///     Prepares the data for logging by reconstructing it according to
    ///     the field names as specified in the DisplayName attribute on the
    ///     properties.
    /// </summary>
    public static IEnumerable<ExpandoObject> ReconstructForIngestion(this object instance)
    {
        var expandoObjects = new List<ExpandoObject>();
        if (instance is IEnumerable enumerable)
        {
            expandoObjects.AddRange(enumerable
                .Cast<object>()
                .Select(CreateExpandoObject)
                .Select(obj => (ExpandoObject)obj));
        }
        else
        {
            expandoObjects.Add(CreateExpandoObject(instance));
        }

        return expandoObjects;
    }

    /// <summary>
    ///     Convert the instances into binary data.
    /// </summary>
    public static BinaryData ToBinaryData(this IEnumerable<ExpandoObject> instances) =>
        BinaryData.FromObjectAsJson(instances);

    /// <summary>
    ///     ToGenericObject will convert the <see cref="LogsQueryResult" /> response into a domain class using reflection.
    /// </summary>
    /// <typeparam name="TClass">The specified domain class.</typeparam>
    /// <param name="logsQueryResult">The <see cref="LogsQueryResult" /> response to extract the data.</param>
    /// <returns>An instance of the specified domain class.</returns>
    public static TClass? ToGenericObject<TClass>(this LogsQueryResult? logsQueryResult) where TClass : class, new()
    {
        if (logsQueryResult == null || !logsQueryResult.AllTables.Any())
        {
            return null;
        }

        var table = logsQueryResult.AllTables[0];

        if (!table.Rows.Any())
        {
            return null;
        }

        var row = table.Rows.Single();

        return ParseQueryEntry<TClass>(
            table.Columns.Select(c => (name: c.Name, type: c.Type.ToString())).ToList(),
            row.ToList());
    }

    /// <summary>
    ///     ToGenericCollection will convert the <see cref="WorkspaceResponse" /> response a collection of the specified
    ///     domain class using reflection.
    /// </summary>
    /// <typeparam name="TClass">The specified domain class.</typeparam>
    /// <param name="workspaceResponse">The <see cref="LogsQueryResult" /> response to extract the data.</param>
    /// <returns>A collection of the specified domain class.</returns>
    public static IEnumerable<TClass> ToGenericCollection<TClass>(this LogsQueryResult? workspaceResponse)
        where TClass : class, new()
    {
        if (workspaceResponse == null || !workspaceResponse.AllTables.Any())
        {
            return Enumerable.Empty<TClass>();
        }

        var table = workspaceResponse.AllTables[0];

        if (!table.Rows.Any())
        {
            return Enumerable.Empty<TClass>();
        }

        return table.Rows.Select(row => ParseQueryEntry<TClass>(
            table.Columns.Select(c => (name: c.Name, type: c.Type.ToString())).ToList(),
            row.ToList()));
    }

    private static dynamic CreateExpandoObject(object instance)
    {
        var type = instance.GetType();
        var properties = type.GetProperties();

        dynamic expandoObject = new ExpandoObject();
        var dictionary = (IDictionary<string, object?>)expandoObject;

        foreach (var propertyInfo in properties)
        {
            var displayAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>(true);
            if (displayAttribute is not { Name: not null })
            {
                continue;
            }

            var names = displayAttribute.Name.Split(',');
            foreach (var name in names)
            {
                dictionary.Add(name.Trim(), propertyInfo.GetValue(instance));
            }
        }

        return expandoObject;
    }

    private static T ParseQueryEntry<T>(
        IList<(string name, string type)> columns,
        IList<object> row) where T : class, new()
    {
        var instance = Activator.CreateInstance<T>();
        var type = instance.GetType();
        var properties = type.GetProperties();

        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            var propertyInfo = Array.Find(properties, prop =>
            {
                var displayAttribute = prop.GetCustomAttribute<DisplayAttribute>(true);
                if (displayAttribute is not { Name: not null })
                {
                    return false;
                }

                var names = displayAttribute.Name.Split(',');
                return Array.Exists(names,
                    name => name.Trim().Equals(column.name, StringComparison.InvariantCultureIgnoreCase));
            });

            if (propertyInfo == null)
            {
                continue;
            }

            var value = ParsePropertyValue(column.type, row[i], propertyInfo);
            propertyInfo.SetValue(instance, value);
        }

        return instance;
    }

    private static object? ParsePropertyValue(string propertyType, object? value, PropertyInfo propertyInfo)
    {
        if (value == null)
        {
            return null;
        }

        var valueStr = value.ToString();

        return propertyType switch
        {
            "datetime" => DateTime.TryParse(valueStr, out var dateTimeResult) ? dateTimeResult : null,
            "bool" => bool.TryParse(valueStr, out var boolResult) ? boolResult : null,
            "real" => ParseRealValue(valueStr, propertyInfo),
            "long" => ParseLongValue(valueStr, propertyInfo),
            "int" => ParseIntValue(valueStr, propertyInfo),
            "string" => ParseStringValue(valueStr, propertyInfo),
            _ => throw new NotSupportedException($"Unsupported propertyType: {propertyType}")
        };
    }

    private static object? ParseRealValue(string? valueStr, PropertyInfo propertyInfo)
    {
        if (valueStr == null)
        {
            return null;
        }

        if (propertyInfo.PropertyType.IsEnum)
        {
            return TryParseEnum(valueStr, propertyInfo);
        }

        return double.TryParse(valueStr, out var doubleResult)
            ? doubleResult
            : null;
    }

    private static object? ParseLongValue(string? valueStr, PropertyInfo propertyInfo)
    {
        if (valueStr == null)
        {
            return null;
        }

        if (propertyInfo.PropertyType.IsEnum)
        {
            return TryParseEnum(valueStr, propertyInfo);
        }

        return long.TryParse(valueStr, out var longResult)
            ? longResult
            : null;
    }

    private static object? ParseIntValue(string? valueStr, PropertyInfo propertyInfo)
    {
        if (valueStr == null)
        {
            return null;
        }

        if (propertyInfo.PropertyType.IsEnum)
        {
            return TryParseEnum(valueStr, propertyInfo);
        }

        return int.TryParse(valueStr, out var intResult)
            ? intResult
            : null;
    }

    private static object? TryParseEnum(string? valueStr, PropertyInfo propertyInfo)
    {
        return Enum.TryParse(propertyInfo.PropertyType, valueStr, true, out var enumResult)
            ? enumResult : null;
    }

    private static object? ParseStringValue(string? valueStr, PropertyInfo propertyInfo)
    {
        if (valueStr == null)
        {
            return null;
        }

        if (propertyInfo.PropertyType == typeof(string))
        {
            return valueStr;
        }

        if (propertyInfo.PropertyType.IsPrimitive)
        {
            return Convert.ChangeType(valueStr, propertyInfo.PropertyType);
        }

        return JsonConvert.DeserializeObject(valueStr, propertyInfo.PropertyType);
    }
}