using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.GraphQL.Helpers;
using GitHubPullRequestMetrics.GraphQL.Models.PullRequestDetails;
using GitHubPullRequestMetrics.GraphQL.Models.Search;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Models;

namespace GitHubPullRequestMetrics.Services;

public class PullRequestMetricsService(
    IGitHubClient client, 
    GitHubOptions options, 
    IMetricsAggregationService aggregationService) 
    : IPullRequestMetricsService
{
    public async Task<Result<MetricsSummaryDto>> GetPullRequestMetricsAsync(DateTime from, DateTime to)
    {
        var metricsResult = await GetMetricsAsync(from, to);

        if (!metricsResult.IsSuccess)
        {
            return Result<MetricsSummaryDto>.Failure(metricsResult.Error!);
        }

        var summary = aggregationService.AggregateMetrics(metricsResult.Value!);

        return Result<MetricsSummaryDto>.Success(summary);
    }

    private async Task<Result<IEnumerable<PullRequestMetricsDto>>> GetMetricsAsync(
        DateTime from,
        DateTime to)
    {

        var searchResult = await SearchPullRequestsAsync(from, to);
        if (!searchResult.IsSuccess)
        {
            return Result<IEnumerable<PullRequestMetricsDto>>.Failure(searchResult.Error!);
        }

        var allMetrics = new List<PullRequestMetricsDto>();

        foreach (var prNode in searchResult.Value!)
        {
            var detailsResult = await GetPullRequestDetailsAsync(prNode.Number);

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
        DateTime from,
        DateTime to)
    {
        var queryString = BuildSearchQuery(from, to);
        var allNodes = new List<PullRequestNode>();
        string? cursor = null;
        bool hasNextPage = true;

        while (hasNextPage)
        {
            var request = QueryBuilder.GetPullRequestSearchQuery(queryString, cursor);
            var result = await client.ExecuteQueryAsync<SearchResponseData>(request);

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
        int prNumber)
    {
        var request = QueryBuilder.GetPullRequestDetailsQuery(
            owner: options.Owner, 
            repo: options.Repository, 
            number: prNumber);
        
        var result = await client.ExecuteQueryAsync<PullRequestDetailsResponseData>(request);

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
        var allReviews = pullRequest.Reviews.Nodes
        .Where(r => r.SubmittedAt != null && r.Author?.Login != author)
        .OrderBy(r => r.SubmittedAt)
        .ToList();

        var firstReview = allReviews.FirstOrDefault();

        var uniqueReviewers = new HashSet<string>();
        DateTime? minimumReviewersReachedAt = null;

        foreach (var review in allReviews)
        {
            var reviewerLogin = review.Author?.Login;
            if (!string.IsNullOrEmpty(reviewerLogin))
            {
                uniqueReviewers.Add(reviewerLogin);

                if (minimumReviewersReachedAt == null &&
                    uniqueReviewers.Count >= options.MinimumReviewers)
                {
                    minimumReviewersReachedAt = review.SubmittedAt;
                }
            }
        }

        var approvals = allReviews
            .Where(r => r.State == "APPROVED")
            .OrderBy(r => r.SubmittedAt)
            .ToList();

        var firstApproval = approvals.FirstOrDefault();

        var uniqueApprovers = new HashSet<string>();
        DateTime? minimumApprovalsReachedAt = null;

        foreach (var approval in approvals)
        {
            var approverLogin = approval.Author?.Login;
            if (!string.IsNullOrEmpty(approverLogin))
            {
                uniqueApprovers.Add(approverLogin);

                if (minimumApprovalsReachedAt == null &&
                    uniqueApprovers.Count >= options.MinimumApprovals)
                {
                    minimumApprovalsReachedAt = approval.SubmittedAt;
                }
            }
        }

        return new PullRequestMetricsDto
        {
            PullRequestNumber = prNumber,
            Title = pullRequest.Title,
            Author = author,
            CreatedAt = pullRequest.CreatedAt,

            FirstReviewAt = firstReview?.SubmittedAt,
            MinimumReviewersReachedAt = minimumReviewersReachedAt,
            TotalReviewersCount = uniqueReviewers.Count,

            FirstApprovalAt = firstApproval?.SubmittedAt,
            MinimumApprovalsReachedAt = minimumApprovalsReachedAt,
            TotalApprovalsCount = uniqueApprovers.Count,

            MergedAt = pullRequest.MergedAt
        };
    }

    private string BuildSearchQuery(DateTime from, DateTime to)
    {
        var queryParts = new List<string>
        {
            $"repo:{options.Owner}/{options.Repository}",
            "is:pr",
            "is:merged",
            $"created:{from:yyyy-MM-dd}..{to:yyyy-MM-dd}"
        };

        if (options.TeamMembers != null && options.TeamMembers.Count > 0)
        {
            foreach (var member in options.TeamMembers)
            {
                queryParts.Add($"author:{member}");
            }
        }

        return string.Join(" ", queryParts);
    }    
}
