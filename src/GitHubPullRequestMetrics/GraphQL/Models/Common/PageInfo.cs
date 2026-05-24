namespace GitHubPullRequestMetrics.GraphQL.Models.Common;

/// <summary>
/// GitHub GraphQL Pagination information
/// </summary>
internal class PageInfo
{
    public bool HasNextPage { get; set; }
    public string? EndCursor { get; set; }
}
