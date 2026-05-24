
namespace GitHubPullRequestMetrics.Configuration;

public class GitHubOptions
{
    public string Token { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    /// <summary>
    /// List of GitHub usernames representing team members.
    /// When specified, only Pull Requests created by these users will be included.
    /// Leave empty to include all users.
    /// </summary>
    public List<string>? TeamMembers { get; set; }

    public int MinimumReviewers { get; set; } = 1;
    public int MinimumApprovals { get; set; } = 1;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Token)) 
            throw new InvalidOperationException("GitHub Token is required. Please configure GitHubOptions.Token.");

        if (string.IsNullOrWhiteSpace(Owner)) 
            throw new InvalidOperationException("Repository Owner is required. Please configure GitHubOptions.Owner");

        if (string.IsNullOrWhiteSpace(Repository))
            throw new InvalidOperationException("Repository is required. Please configure GithubOptions.Repository");

        if (MinimumReviewers < 1)
            throw new InvalidOperationException("MinimumReviewers must be at least 1.");

        if (MinimumApprovals < 1)
            throw new InvalidOperationException("MinimumApprovals must be at least 1.");
    }
}
