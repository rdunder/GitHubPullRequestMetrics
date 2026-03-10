using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Models.Metrics;

/// <summary>
/// Contains calculated metrics for a single Pull Request.
/// Represents the time taken for various stages in the PR lifecycle.
/// </summary>
public class PullRequestMetricsDto
{
    public int PullRequestNumber { get; set; }
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? FirstReviewAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? MergedAt { get; set; }
    public TimeSpan? TimeToFirstReview =>
        FirstReviewAt.HasValue ? FirstReviewAt.Value - CreatedAt : null;
    public TimeSpan? TimeToApproval =>
        ApprovedAt.HasValue ? ApprovedAt.Value - CreatedAt : null;
    public TimeSpan? TimeToMerge =>
        MergedAt.HasValue ? MergedAt.Value - CreatedAt : null;
    public TimeSpan? ReviewToApprovalTime =>
        FirstReviewAt.HasValue && ApprovedAt.HasValue
            ? ApprovedAt.Value - FirstReviewAt.Value
            : null;
}
