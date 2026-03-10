using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Models.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Services;

public class PullrequestMetricsService : IPullrequestMetricsService
{
    public Task<IEnumerable<PullRequestMetricsDto>> GetMetricsAsync(DateTime from, DateTime to, string? owner = null, string? repository = null)
    {
        throw new NotImplementedException();
    }
}
