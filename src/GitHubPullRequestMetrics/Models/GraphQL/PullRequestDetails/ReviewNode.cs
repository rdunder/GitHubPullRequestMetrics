using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.PullRequestDetails;

/// <summary>
/// Represents a single code review on a Pull Request.
/// </summary>
internal class ReviewNode
{
    /// <summary>
    /// The state of the review.
    /// Common values: "APPROVED", "CHANGES_REQUESTED", "COMMENTED", "DISMISSED"
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the review was submitted.
    /// Null if the review has not been submitted yet.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }
}
