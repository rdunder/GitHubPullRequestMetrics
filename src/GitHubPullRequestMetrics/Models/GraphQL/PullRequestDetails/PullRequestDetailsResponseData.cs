using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.PullRequestDetails;

internal class PullRequestDetailsResponseData
{
    public Repository Repository { get; set; } = new();
}
