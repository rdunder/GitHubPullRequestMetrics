using GitHubPullRequestMetrics.Models.GraphQL.Common;
using GitHubPullRequestMetrics.Models.GraphQL.PullRequestDetails;
using GitHubPullRequestMetrics.Models.Metrics;

namespace GitHubPullRequestMetrics.Interfaces;

public interface IPullRequestMetricsService
{
    /// <summary>
    /// Gets Pull Request metrics for a repository within a date range.
    /// Uses default owner and repository from configuration if not specified.
    /// </summary>
    /// <param name="from">Start date for PRs (inclusive).</param>
    /// <param name="to">End date for PRs (inclusive).</param>
    /// <param name="owner">Repository owner (organization or user). Uses DefaultOwner from config if null.</param>
    /// <param name="repository">Repository name. Uses DefaultRepository from config if null.</param>
    /// <returns>Collection of PR metrics with calculated time durations.</returns>
    Task<Result<IEnumerable<PullRequestMetricsDto>>> GetMetricsAsync(
        DateTime from,
        DateTime to,
        string? owner = null,
        string? repository = null);

    /// <summary>
    /// Gets aggregated Pull Request metrics summary with statistics for a repository within a date range.
    /// Includes individual PR data plus calculated averages, medians, and counts.
    /// Uses default owner and repository from configuration if not specified.
    /// </summary>
    /// <param name="from">Start date for PRs (inclusive).</param>
    /// <param name="to">End date for PRs (inclusive).</param>
    /// <param name="owner">Repository owner (organization or user). Uses DefaultOwner from config if null.</param>
    /// <param name="repository">Repository name. Uses DefaultRepository from config if null.</param>
    /// <returns>A Result containing either the aggregated summary or an error message.</returns>
    Task<Result<MetricsSummaryDto>> GetMetricsSummaryAsync(
        DateTime from,
        DateTime to,
        string? owner = null,
        string? repository = null);
}
