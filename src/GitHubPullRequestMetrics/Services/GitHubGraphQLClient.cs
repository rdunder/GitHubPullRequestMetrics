using System.Net.Http.Headers;

using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Models;

using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace GitHubPullRequestMetrics.Services;

public class GitHubGraphQLClient : IGitHubClient
{
	private readonly GraphQLHttpClient _graphQLClient;
	private const string GitHubGraphQLEndpoint = "https://api.github.com/graphql";

	public GitHubGraphQLClient(IHttpClientFactory httpClientFactory, GitHubOptions options)
	{
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

	public async Task<Result<T>> ExecuteQueryAsync<T>(GraphQLRequest request)
	{
		try
		{
			var response = await _graphQLClient.SendQueryAsync<T>(request);

			if (response.Errors != null && response.Errors.Length > 0)
			{
				var errorMessages = string.Join("; ", response.Errors.Select(e => e.Message));
				return Result<T>.Failure($"GraphQL errors: {errorMessages}");
			}

			if (response.Data == null)
				return Result<T>.Failure("GraphQL query returned null data with no errors.");

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
}
