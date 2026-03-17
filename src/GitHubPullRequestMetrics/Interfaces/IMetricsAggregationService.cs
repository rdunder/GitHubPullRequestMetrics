using GitHubPullRequestMetrics.Models;

namespace GitHubPullRequestMetrics.Interfaces;

public interface IMetricsAggregationService
{
    public MetricsSummaryDto AggregateMetrics(IEnumerable<PullRequestMetricsDto> metrics);
}
