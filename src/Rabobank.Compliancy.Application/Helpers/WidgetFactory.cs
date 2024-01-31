#nullable enable

namespace Rabobank.Compliancy.Application.Helpers;

public static class WidgetFactory
{
    private const string _purple = "#68217A";
    private const string _orange = "#F2700F";
    private const string _red = "#DA0A00";

    private static string DetermineBackgroundColor(bool isSuccess) => isSuccess ? _purple : _red;

    private static string DetermineBackgroundColor(long count)
    {
        return count switch
        {
            >= 10 => _red,
            >= 1 => _orange,
            _ => _purple,
        };
    }

    public static string CreateDiagnosticsWidgetContent(string title, bool isSuccess, string version) =>
        CreateWidgetContent(title, DetermineBackgroundColor(isSuccess), version);

    public static string CreateFoundRecordsWidgetContent(string title, bool? isSuccess, long foundRecordsCount)
    {
        var text = $"Found {foundRecordsCount} records in the last 24 hours";

        return isSuccess == null
            ? CreateWidgetContent(title, DetermineBackgroundColor(foundRecordsCount), text)
            : CreateWidgetContent(title, DetermineBackgroundColor(isSuccess.Value), text);
    }

    public static string CreateWidgetContent(string title, string backgroundColor, string htmlContent)
    {
        const string fontFamily = "\"Segoe UI VSS(Regular)\",\"Segoe UI\",\" - apple - system\",BlinkMacSystemFont,Roboto,\"Helvetica Neue\",Helvetica,Ubuntu,Arial,sans-serif,\"Apple Color Emoji\",\"Segoe UI Emoji\",\"Segoe UI Symbol\"";
        var bodyStyle = $"font-family:{fontFamily};font-size:12px;color:white;background-color:{backgroundColor}";
        const string titleStyle = "font-size:16px;font-weight:normal;";

        return $@"<html>
                        <body style='{bodyStyle}'>
                          <h1 style='{titleStyle}'>
                            {title}
                          </h1>
                          <p>{htmlContent}</p>
                        </body>
                      </html>";
    }
}