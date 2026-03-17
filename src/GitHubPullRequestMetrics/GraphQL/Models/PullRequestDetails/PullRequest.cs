namespace GitHubPullRequestMetrics.GraphQL.Models.PullRequestDetails;

internal class PullRequest
{
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? MergedAt { get; set; }
    public ReviewConnection Reviews { get; set; } = new();
}
