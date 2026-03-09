using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Models.GraphQL.Search;

internal class Author
{
    /// <summary>
    /// GitHub username (login) of the author.
    /// </summary>
    public string Login { get; set; } = string.Empty;
}
