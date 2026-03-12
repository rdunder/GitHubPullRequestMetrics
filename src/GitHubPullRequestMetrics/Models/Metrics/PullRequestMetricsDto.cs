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
    public DateTime? MinimumReviewersReachedAt { get; set; }
    public int TotalReviewersCount { get; set; }
    public DateTime? FirstApprovalAt { get; set; }
    public DateTime? MinimumApprovalsReachedAt { get; set; }
    public int TotalApprovalsCount { get; set; }
    public DateTime? MergedAt { get; set; }

    
    // ___ Calculated Properties ___

    public TimeSpan? TimeToFirstReview =>
        FirstReviewAt.HasValue ? FirstReviewAt.Value - CreatedAt : null;

    public TimeSpan? TimeToMinimumReviewers =>
        MinimumReviewersReachedAt.HasValue
            ? MinimumReviewersReachedAt.Value - CreatedAt
            : null;

    public TimeSpan? TimeToFirstApproval =>
        FirstApprovalAt.HasValue ? FirstApprovalAt.Value - CreatedAt : null;

    public TimeSpan? TimeToMinimumApprovals =>
        MinimumApprovalsReachedAt.HasValue
            ? MinimumApprovalsReachedAt.Value - CreatedAt
            : null;

    public TimeSpan? TimeToMerge =>
        MergedAt.HasValue ? MergedAt.Value - CreatedAt : null;

    public TimeSpan? ReviewToApprovalTime =>
        FirstReviewAt.HasValue && FirstApprovalAt.HasValue
            ? FirstApprovalAt.Value - FirstReviewAt.Value
            : null;
}
