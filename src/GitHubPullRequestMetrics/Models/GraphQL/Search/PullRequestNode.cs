using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.Search;

/// <summary>
/// Represents a single Pull Request node returned from a GitHub search query.
/// Contains basic PR information without detailed review data.
/// </summary>
internal class PullRequestNode
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? MergedAt { get; set; }
    public Author Author { get; set; } = new();
}
