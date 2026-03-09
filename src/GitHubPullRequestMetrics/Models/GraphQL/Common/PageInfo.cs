using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.Common;

/// <summary>
/// GitHub GraphQL Pagination information
/// </summary>
internal class PageInfo
{
    public bool HasNextPage { get; set; }
    public string? EndCursor { get; set; }
}
