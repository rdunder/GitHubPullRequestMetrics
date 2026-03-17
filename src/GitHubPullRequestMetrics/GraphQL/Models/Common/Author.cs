namespace GitHubPullRequestMetrics.GraphQL.Models.Common;

internal class Author
{
    /// <summary>
    /// GitHub username (login) of the author.
    /// </summary>
    public string Login { get; set; } = string.Empty;
}
