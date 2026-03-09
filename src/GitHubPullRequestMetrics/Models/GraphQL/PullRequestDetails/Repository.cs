using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.PullRequestDetails;

internal class Repository
{
    public PullRequest PullRequest { get; set; } = new();
}
