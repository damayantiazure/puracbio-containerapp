using System;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class QueryByWiql
{
    public string Query { get; }

    public QueryByWiql(string query)
    {
        Query = query ?? throw new ArgumentNullException(nameof(query));
    }
}