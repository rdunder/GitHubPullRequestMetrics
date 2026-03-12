using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Models.Metrics;

namespace GitHubPullRequestMetrics.Services;

public class MetricsAggregationService(GitHubOptions options) : IMetricsAggregationService
{
    private readonly GitHubOptions _options = options;

    public MetricsSummaryDto AggregateMetrics(IEnumerable<PullRequestMetricsDto> metrics)
    {
        var metricsList = metrics.ToList();

        return new MetricsSummaryDto
        {
            PullRequests = metricsList.AsReadOnly(),

            TotalPRs = metricsList.Count,
            PRsWithReviews = metricsList.Count(m => m.FirstReviewAt.HasValue),
            PRsWithMinimumReviewers = metricsList.Count(m => m.MinimumReviewersReachedAt.HasValue),
            PRsWithApprovals = metricsList.Count(m => m.FirstApprovalAt.HasValue),
            PRsWithMinimumApprovals = metricsList.Count(m => m.MinimumApprovalsReachedAt.HasValue),

            AverageTimeToFirstReview = CalculateAverage(metricsList.Select(m => m.TimeToFirstReview)),
            MedianTimeToFirstReview = CalculateMedian(metricsList.Select(m => m.TimeToFirstReview)),

            AverageTimeToMinimumReviewers = CalculateAverage(metricsList.Select(m => m.TimeToMinimumReviewers)),
            MedianTimeToMinimumReviewers = CalculateMedian(metricsList.Select(m => m.TimeToMinimumReviewers)),

            AverageTimeToFirstApproval = CalculateAverage(metricsList.Select(m => m.TimeToFirstApproval)),
            MedianTimeToFirstApproval = CalculateMedian(metricsList.Select(m => m.TimeToFirstApproval)),

            AverageTimeToMinimumApprovals = CalculateAverage(metricsList.Select(m => m.TimeToMinimumApprovals)),
            MedianTimeToMinimumApprovals = CalculateMedian(metricsList.Select(m => m.TimeToMinimumApprovals)),

            AverageTimeToMerge = CalculateAverage(metricsList.Select(m => m.TimeToMerge)),
            MedianTimeToMerge = CalculateMedian(metricsList.Select(m => m.TimeToMerge))
        };
    }

    private static TimeSpan? CalculateAverage(IEnumerable<TimeSpan?> values)
    {
        var validValues = values.Where(v => v.HasValue).Select(v => v!.Value).ToList();

        if (validValues.Count == 0)
            return null;

        var averageTicks = (long)validValues.Average(ts => ts.Ticks);
        return TimeSpan.FromTicks(averageTicks);
    }

    private static TimeSpan? CalculateMedian(IEnumerable<TimeSpan?> values)
    {
        var sorted = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .OrderBy(ts => ts.Ticks)
            .ToList();

        if (sorted.Count == 0)
            return null;

        int midpoint = sorted.Count / 2;

        if (sorted.Count % 2 == 0)
        {
            var middleTicks = (sorted[midpoint - 1].Ticks + sorted[midpoint].Ticks) / 2;
            return TimeSpan.FromTicks(middleTicks);
        }

        return sorted[midpoint];
    }
}
