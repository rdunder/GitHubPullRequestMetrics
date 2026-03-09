using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.PullRequestDetails;

internal class PullRequest
{
    public DateTime CreatedAt { get; set; }
    public DateTime? MergedAt { get; set; }
    public ReviewConnection Reviews { get; set; } = new();
}
