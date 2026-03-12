using GitHubPullRequestMetrics.Models.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Interfaces;

public interface IMetricsAggregationService
{
    public MetricsSummaryDto AggregateMetrics(IEnumerable<PullRequestMetricsDto> metrics);
}
