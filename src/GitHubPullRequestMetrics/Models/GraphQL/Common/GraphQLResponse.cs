using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.Common;

internal class GraphQLResponse<T>
{
    public T? Data { get; set; }
    public List<GraphQLError>? Errors { get; set; }
}

internal class GraphQLError
{
    public string Message { get; set; } = string.Empty;
    public List<ErrorLocation>? Locations { get; set; }
}

internal class ErrorLocation
{
    public int Line { get; set; }
    public int Column { get; set; }
}
