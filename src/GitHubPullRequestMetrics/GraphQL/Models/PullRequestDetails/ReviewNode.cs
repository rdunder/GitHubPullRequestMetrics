using GitHubPullRequestMetrics.GraphQL.Models.Common;

namespace GitHubPullRequestMetrics.GraphQL.Models.PullRequestDetails;

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

    public DateTime? SubmittedAt { get; set; }

    public Author? Author { get; set; }
}
