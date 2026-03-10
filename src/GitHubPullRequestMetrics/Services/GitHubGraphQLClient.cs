using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Models.GraphQL.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GitHubPullRequestMetrics.Services;

internal class GitHubGraphQLClient : IGitHubClient
{
    private readonly HttpClient _httpClient;
    private const string GitHubGraphQLEndpoint = "https://api.github.com/graphql";

    public GitHubGraphQLClient(IHttpClientFactory httpClientFactory, GitHubOptions options)
    {
        options.Validate();

        _httpClient = httpClientFactory.CreateClient("GitHub");
        _httpClient.BaseAddress = new Uri(GitHubGraphQLEndpoint);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", options.Token);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GithubPullRequestMetrics");
    }

    public async Task<Result<T>> ExecuteQueryAsync<T>(string query, object? variables = null)
    {
        try
        {
            var requestBody = new
            {
                query = query,
                variables = variables
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("", httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return Result<T>.Failure(
                    $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var graphqlResponse = JsonSerializer.Deserialize<GraphQLResponse<T>>(
                responseContent,
                options);

            if (graphqlResponse == null)
            {
                return Result<T>.Failure(
                    $"Failed to deserialize GraphQL response. Raw response: {responseContent}");
            }

            if (graphqlResponse.Errors != null && graphqlResponse.Errors.Count > 0)
            {
                var errorMessages = string.Join("; ", graphqlResponse.Errors.Select(e => e.Message));
                return Result<T>.Failure($"GraphQL errors: {errorMessages}");
            }

            if (graphqlResponse.Data == null)
            {
                return Result<T>.Failure(
                    $"GraphQL query returned null data with no errors. Response: {responseContent}");
            }

            return Result<T>.Success(graphqlResponse.Data);
        }
        catch (HttpRequestException ex)
        {
            return Result<T>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return Result<T>.Failure($"JSON deserialization failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }
}
}
