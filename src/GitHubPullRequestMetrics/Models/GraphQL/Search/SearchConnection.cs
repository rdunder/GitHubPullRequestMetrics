using GitHubPullRequestMetrics.Models.GraphQL.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.Search;

/// <summary>
/// Represents the connection structure returned by GitHub's search query.
/// Contains a list of Pull Request nodes and pagination information.
/// </summary>
internal class SearchConnection
{
    public List<PullRequestNode> Nodes { get; set; } = [];

    public PageInfo PageInfo { get; set; } = new();
}
