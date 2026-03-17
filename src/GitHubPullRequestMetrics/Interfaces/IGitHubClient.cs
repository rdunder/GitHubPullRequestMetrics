using GitHubPullRequestMetrics.Models;
using GraphQL;

namespace GitHubPullRequestMetrics.Interfaces;

public interface IGitHubClient
{
    /// <summary>
    /// Executes a GraphQL query and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response data into.</typeparam>
    /// <param name="request">The GraphQL request containing the query and any associated variables.</param>
    /// <returns>The deserialized response data.</returns>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    Task<Result<T>> ExecuteQueryAsync<T>(GraphQLRequest request);
}
