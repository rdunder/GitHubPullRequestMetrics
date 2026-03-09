using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.PullRequestDetails;

internal class ReviewConnection
{
    public List<ReviewNode> Nodes { get; set; } = [];
}
