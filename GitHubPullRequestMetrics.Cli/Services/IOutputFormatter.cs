using GitHubPullRequestMetrics.Models.Metrics;

namespace GitHubPullRequestMetrics.Cli.Services;

internal interface IOutputFormatter
{
    void DisplaySummary(MetricsSummaryDto summary);
    void DisplayIndividualMetrics(IEnumerable<PullRequestMetricsDto> metrics);
}
