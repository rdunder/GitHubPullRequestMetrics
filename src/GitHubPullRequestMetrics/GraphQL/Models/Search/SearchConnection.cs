using GitHubPullRequestMetrics.GraphQL.Models.Common;

namespace GitHubPullRequestMetrics.GraphQL.Models.Search;

/// <summary>
/// Represents the connection structure returned by GitHub's search query.
/// Contains a list of Pull Request nodes and pagination information.
/// </summary>
internal class SearchConnection
{
    public List<PullRequestNode> Nodes { get; set; } = [];

    public PageInfo PageInfo { get; set; } = new();
}
