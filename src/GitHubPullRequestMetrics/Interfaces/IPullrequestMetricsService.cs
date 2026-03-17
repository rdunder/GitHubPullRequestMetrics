using GitHubPullRequestMetrics.Models;

namespace GitHubPullRequestMetrics.Interfaces;

public interface IPullRequestMetricsService
{
    /// <summary>
    /// Gets aggregated Pull Request metrics summary with statistics for a repository within a date range.
    /// Includes individual PR data plus calculated averages, medians, and counts.
    /// Uses default owner and repository from configuration if not specified.
    /// </summary>
    /// <param name="from">Start date for PRs (inclusive).</param>
    /// <param name="to">End date for PRs (inclusive).</param>
    /// <param name="owner">Repository owner (organization or user). Uses Owner from config if null.</param>
    /// <param name="repository">Repository name. Uses Repository from config if null.</param>
    /// <returns>A Result containing either the aggregated summary or an error message.</returns>
    Task<Result<MetricsSummaryDto>> GetPullRequestMetricsAsync(
        DateTime from,
        DateTime to);
}
