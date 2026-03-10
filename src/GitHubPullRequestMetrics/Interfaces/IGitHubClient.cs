using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubPullRequestMetrics.Interfaces;

internal interface IGitHubClient
{
    /// <summary>
    /// Executes a GraphQL query and returns the deserialized response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response data into.</typeparam>
    /// <param name="query">The GraphQL query string.</param>
    /// <param name="variables">Optional variables for the query.</param>
    /// <returns>The deserialized response data.</returns>
    /// <exception cref="HttpRequestException">Thrown when the API request fails.</exception>
    Task<T> ExecuteQueryAsync<T>(string query, object? variables = null);
}
