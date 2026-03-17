namespace GitHubPullRequestMetrics.GraphQL.Models.Search;

/// <summary>
/// Root response object for GitHub GraphQL search queries.
/// Wraps the search connection data.
/// </summary>
internal class SearchResponseData
{
    public SearchConnection Search { get; set; } = new();
}
