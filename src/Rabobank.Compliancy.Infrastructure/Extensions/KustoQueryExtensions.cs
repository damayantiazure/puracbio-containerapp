using System.Text.RegularExpressions;

namespace Rabobank.Compliancy.Infrastructure.Extensions;

public static class KustoQueryExtensions
{
    /// <summary>
    ///     Searches the given kusto query and transforms existing source and union statements
    ///     to wildcard pattern union statements that include all tables that match the pattern
    ///     into the query.
    /// </summary>
    public static string ToWildCardUnion(this string kustoQuery) =>
        UnionStatementsToWildCard(SourceStatementToWildCardUnion(kustoQuery));

    /// <summary>
    ///     This method selects the source statement from a kusto query and transforms it to
    ///     a wildcard union.
    ///     <example>
    ///         For example:
    ///         <code>log_table_CL | summarize count() by Type</code>
    ///         Would translate to:
    ///         <code>union log_table*_CL | summarize count() by Type</code>
    ///     </example>
    ///     This means that, given the following source statement:
    ///     <code>log_table_CL</code>
    ///     The following tables will be included in the query:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>log_table2_CL</description>
    ///         </item>
    ///         <item>
    ///             <description>log_table3_CL</description>
    ///         </item>
    ///         <item>
    ///             <description>log_table4_CL</description>
    ///         </item>
    ///         <item>
    ///             <description>etc.</description>
    ///         </item>
    ///     </list>
    /// </summary>
    private static string SourceStatementToWildCardUnion(string kustoQuery)
    {
        const string pattern = "^\\s*(?<capture>[a-z0-9_]*)_CL";
        const string replacement = "union ${capture}*_CL";
        return Regex.Replace(kustoQuery, pattern, replacement);
    }

    /// <summary>
    ///     This method selects the union statements from a kusto query and transforms
    ///     them to a wildcard union statement.
    ///     <example>
    ///         For example:
    ///         <code>union log_table_CL</code>
    ///         Would translate to:
    ///         <code>union log_table*_CL</code>
    ///     </example>
    ///     This means that, given the following union statement:
    ///     <code>union log_table_CL</code>
    ///     The following tables will be included in the query:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>log_table2_CL</description>
    ///         </item>
    ///         <item>
    ///             <description>log_table3_CL</description>
    ///         </item>
    ///         <item>
    ///             <description>log_table4_CL</description>
    ///         </item>
    ///         <item>
    ///             <description>etc.</description>
    ///         </item>
    ///     </list>
    /// </summary>
    private static string UnionStatementsToWildCard(string kustoQuery)
    {
        const string pattern = "\\b(?<capture>union\\s+[a-z0-9_]*)_CL";
        const string replacement = "${capture}*_CL";
        return Regex.Replace(kustoQuery, pattern, replacement);
    }
}