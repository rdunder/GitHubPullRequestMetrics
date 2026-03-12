
namespace GitHubPullRequestMetrics.Models.Metrics;

public class MetricsSummaryDto
{
    public IReadOnlyList<PullRequestMetricsDto> PullRequests { get; set; } = [];


    public int TotalPRs { get; set; }

    public int PRsWithReviews { get; set; }

    public int PRsWithMinimumReviewers { get; set; }

    public int PRsWithApprovals { get; set; }

    public int PRsWithMinimumApprovals { get; set; }


    public TimeSpan? AverageTimeToFirstReview { get; set; }

    public TimeSpan? MedianTimeToFirstReview { get; set; }


    public TimeSpan? AverageTimeToMinimumReviewers { get; set; }

    public TimeSpan? MedianTimeToMinimumReviewers { get; set; }


    public TimeSpan? AverageTimeToFirstApproval { get; set; }

    public TimeSpan? MedianTimeToFirstApproval { get; set; }


    public TimeSpan? AverageTimeToMinimumApprovals { get; set; }

    public TimeSpan? MedianTimeToMinimumApprovals { get; set; }


    public TimeSpan? AverageTimeToMerge { get; set; }

    public TimeSpan? MedianTimeToMerge { get; set; }
}
