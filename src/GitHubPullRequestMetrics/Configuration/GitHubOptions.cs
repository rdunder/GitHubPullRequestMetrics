using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Configuration;

public class GitHubOptions
{
    public string Token { get; set; } = string.Empty;

    public string DefaultOwner { get; set; } = string.Empty;

    public string DefaultRepository { get; set; } = string.Empty;

    /// <summary>
    /// List of GitHub usernames representing team members.
    /// When specified, only Pull Requests created by these users will be included.
    /// Leave empty to include all users.
    /// </summary>
    public List<string>? TeamMembers { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Token)) 
            throw new InvalidOperationException("GitHub Token is required. Please configure GitHubOptions.Token.");

        if (string.IsNullOrWhiteSpace(DefaultOwner)) 
            throw new InvalidOperationException("Repository Owner is required. Please configure GitHubOptions.DefaultOwner");

        if (string.IsNullOrWhiteSpace(DefaultRepository))
            throw new InvalidOperationException("Repository is required. Please configure GithubOptions.DefaultRepository");
    }
}
