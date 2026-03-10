using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Models.GraphQL.Common;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GitHubPullRequestMetrics.Services;

public class GitHubGraphQLClient : IGitHubClient
{
    private readonly GraphQLHttpClient _graphQLClient;
    private const string GitHubGraphQLEndpoint = "https://api.github.com/graphql";

    public GitHubGraphQLClient(IHttpClientFactory httpClientFactory, GitHubOptions options)
    {
        options.Validate();

        var httpClient = httpClientFactory.CreateClient("GitHub");
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.Token);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "GithubPullRequestMetrics");

        var graphQLOptions = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(GitHubGraphQLEndpoint)
        };

        _graphQLClient = new GraphQLHttpClient(
            graphQLOptions,
            new SystemTextJsonSerializer(),
            httpClient);
    }

    public async Task<Result<T>> ExecuteQueryAsync<T>(string query, object? variables = null)
    {
        try
        {
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = variables
            };

            var response = await _graphQLClient.SendQueryAsync<T>(request);

            if (response.Errors != null && response.Errors.Length > 0)
            {
                var errorMessages = string.Join("; ", response.Errors.Select(e => e.Message));
                return Result<T>.Failure($"GraphQL errors: {errorMessages}");
            }

            if (response.Data == null)
            {
                return Result<T>.Failure("GraphQL query returned null data with no errors.");
            }

            return Result<T>.Success(response.Data);
        }
        catch (GraphQLHttpRequestException ex)
        {
            return Result<T>.Failure($"GraphQL HTTP request failed: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return Result<T>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _graphQLClient?.Dispose();
    }
}
