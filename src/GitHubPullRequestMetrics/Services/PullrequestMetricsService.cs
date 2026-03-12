using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Models.GraphQL.Common;
using GitHubPullRequestMetrics.Models.GraphQL.PullRequestDetails;
using GitHubPullRequestMetrics.Models.GraphQL.Search;
using GitHubPullRequestMetrics.Models.Metrics;

namespace GitHubPullRequestMetrics.Services;

public class PullRequestMetricsService(IGitHubClient client, GitHubOptions options) : IPullRequestMetricsService
{
    private readonly IGitHubClient _client = client;
    private readonly GitHubOptions _options = options;

    public async Task<Result<IEnumerable<PullRequestMetricsDto>>> GetMetricsAsync(
        DateTime from,
        DateTime to,
        string? owner = null,
        string? repository = null)
    {
        var actualOwner = owner ?? _options.DefaultOwner;
        var actualRepo = repository ?? _options.DefaultRepository;

        var searchResult = await SearchPullRequestsAsync(actualOwner, actualRepo, from, to);
        if (!searchResult.IsSuccess)
        {
            return Result<IEnumerable<PullRequestMetricsDto>>.Failure(searchResult.Error!);
        }

        var allMetrics = new List<PullRequestMetricsDto>();

        foreach (var prNode in searchResult.Value!)
        {
            var detailsResult = await GetPullRequestDetailsAsync(actualOwner, actualRepo, prNode.Number);

            if (!detailsResult.IsSuccess)
            {
                // Log error but continue with other PRs
                // For now, we'll skip failed PRs
                // TODO: Add logging here
                continue;
            }

            var metrics = CalculateMetrics(prNode.Number, prNode.Author.Login, detailsResult.Value!);
            allMetrics.Add(metrics);
        }

        return Result<IEnumerable<PullRequestMetricsDto>>.Success(allMetrics);
    }

    private async Task<Result<IEnumerable<PullRequestNode>>> SearchPullRequestsAsync(
        string owner,
        string repository,
        DateTime from,
        DateTime to)
    {
        var queryString = BuildSearchQuery(owner, repository, from, to);

        var query = @"
            query ($query: String!, $cursor: String) {
                search(type: ISSUE, query: $query, first: 100, after: $cursor) {
                    nodes {
                        ... on PullRequest {
                            number
                            title
                            createdAt
                            mergedAt
                            author {
                                login
                            }
                        }
                    }
                    pageInfo {
                        hasNextPage
                        endCursor
                    }
                }
            }";

        var allNodes = new List<PullRequestNode>();
        string? cursor = null;
        bool hasNextPage = true;

        while (hasNextPage)
        {
            var variables = new { query = queryString, cursor };
            var result = await _client.ExecuteQueryAsync<SearchResponseData>(query, variables);

            if (!result.IsSuccess)
            {
                return Result<IEnumerable<PullRequestNode>>.Failure(result.Error!);
            }

            var data = result.Value!;
            allNodes.AddRange(data.Search.Nodes);

            hasNextPage = data.Search.PageInfo.HasNextPage;
            cursor = data.Search.PageInfo.EndCursor;
        }

        return Result<IEnumerable<PullRequestNode>>.Success(allNodes);
    }

    private async Task<Result<PullRequest>> GetPullRequestDetailsAsync(
        string owner,
        string repository,
        int prNumber)
    {
        var query = @"
            query ($owner: String!, $repo: String!, $number: Int!) {
                repository(owner: $owner, name: $repo) {
                    pullRequest(number: $number) {
                        createdAt
                        mergedAt
                        reviews(first: 100) {
                            nodes {
                                state
                                submittedAt
                            }
                        }
                    }
                }
            }";

        var variables = new { owner, repo = repository, number = prNumber };
        var result = await _client.ExecuteQueryAsync<PullRequestDetailsResponseData>(query, variables);

        if (!result.IsSuccess)
        {
            return Result<PullRequest>.Failure(result.Error!);
        }

        return Result<PullRequest>.Success(result.Value!.Repository.PullRequest);
    }

    private PullRequestMetricsDto CalculateMetrics(
        int prNumber,
        string author,
        PullRequest pullRequest)
    {
        var firstReview = pullRequest.Reviews.Nodes
            .Where(r => r.SubmittedAt != null)
            .OrderBy(r => r.SubmittedAt)
            .FirstOrDefault();

        var approval = pullRequest.Reviews.Nodes
            .Where(r => r.State == "APPROVED" && r.SubmittedAt != null)
            .OrderBy(r => r.SubmittedAt)
            .FirstOrDefault();

        return new PullRequestMetricsDto
        {
            PullRequestNumber = prNumber,
            Author = author,
            CreatedAt = pullRequest.CreatedAt,
            FirstReviewAt = firstReview?.SubmittedAt,
            FirstApprovalAt = approval?.SubmittedAt,
            MergedAt = pullRequest.MergedAt
        };
    }

    private string BuildSearchQuery(string owner, string repository, DateTime from, DateTime to)
    {
        var queryParts = new List<string>
        {
            $"repo:{owner}/{repository}",
            "is:pr",
            "is:merged",
            $"created:{from:yyyy-MM-dd}..{to:yyyy-MM-dd}"
        };

        // Add team member filters if specified
        if (_options.TeamMembers != null && _options.TeamMembers.Count > 0)
        {
            foreach (var member in _options.TeamMembers)
            {
                queryParts.Add($"author:{member}");
            }
        }

        return string.Join(" ", queryParts);
    }
}
