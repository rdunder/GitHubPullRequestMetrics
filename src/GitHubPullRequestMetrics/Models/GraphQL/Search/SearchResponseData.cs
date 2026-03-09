using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.Search;

/// <summary>
/// Root response object for GitHub GraphQL search queries.
/// Wraps the search connection data.
/// </summary>
internal class SearchResponseData
{
    public SearchConnection Search { get; set; } = new();
}
