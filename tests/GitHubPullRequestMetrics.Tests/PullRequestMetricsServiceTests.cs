using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Models.GraphQL.Common;
using GitHubPullRequestMetrics.Models.GraphQL.PullRequestDetails;
using GitHubPullRequestMetrics.Models.GraphQL.Search;
using GitHubPullRequestMetrics.Services;
using Moq;

namespace GitHubPullRequestMetrics.Tests;


public class PullRequestMetricsServiceTests
{
    // ---------- helpers ----------

    private static GitHubOptions CreateOptions(int minReviewers = 2, int minApprovals = 2) =>
        new()
        {
            Token = "test-token",
            DefaultOwner = "test-owner",
            DefaultRepository = "test-repo",
            MinimumReviewers = minReviewers,
            MinimumApprovals = minApprovals
        };

    private static Mock<IGitHubClient> CreateMockClient(
        SearchResponseData? searchResponse = null,
        PullRequestDetailsResponseData? detailsResponse = null)
    {
        var mock = new Mock<IGitHubClient>();

        if (searchResponse != null)
        {
            mock.Setup(c => c.ExecuteQueryAsync<SearchResponseData>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(Result<SearchResponseData>.Success(searchResponse));
        }

        if (detailsResponse != null)
        {
            mock.Setup(c => c.ExecuteQueryAsync<PullRequestDetailsResponseData>(
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .ReturnsAsync(Result<PullRequestDetailsResponseData>.Success(detailsResponse));
        }

        return mock;
    }

    private static SearchResponseData CreateSearchResponse(params PullRequestNode[] nodes)
    {
        return new SearchResponseData
        {
            Search = new SearchConnection
            {
                Nodes = nodes.ToList(),
                PageInfo = new PageInfo
                {
                    HasNextPage = false,
                    EndCursor = null
                }
            }
        };
    }

    private static PullRequestNode CreatePRNode(
        int number,
        string author,
        double hoursAfterBase = 0,
        double? mergedAfterBase = null)
    {
        var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        return new PullRequestNode
        {
            Number = number,
            Title = $"PR {number}",
            Author = new Author { Login = author },
            CreatedAt = baseTime.AddHours(hoursAfterBase),
            MergedAt = mergedAfterBase.HasValue ? baseTime.AddHours(mergedAfterBase.Value) : null
        };
    }

    private static PullRequestDetailsResponseData CreateDetailsResponse(
        DateTime? createdAt = null,
        DateTime? mergedAt = null,
        params ReviewNode[] reviews)
    {
        return new PullRequestDetailsResponseData
        {
            Repository = new Repository
            {
                PullRequest = new PullRequest
                {
                    CreatedAt = createdAt ?? new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                    MergedAt = mergedAt,
                    Reviews = new ReviewConnection
                    {
                        Nodes = reviews.ToList()
                    }
                }
            }
        };
    }

    private static PullRequestMetricsService CreateService(
    GitHubOptions options,
    IGitHubClient client)
    {
        var aggregation = Mock.Of<IMetricsAggregationService>();

        return new PullRequestMetricsService(
            client,
            options,
            aggregation);
    }

    private static ReviewNode CreateReview(
        string authorLogin,
        double hoursAfterCreation,
        string state = "COMMENTED")
    {
        var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        return new ReviewNode
        {
            Author = new Author { Login = authorLogin },
            SubmittedAt = baseTime.AddHours(hoursAfterCreation),
            State = state
        };
    }

    // ---------- reviewer tests ----------

    [Theory]
    [InlineData(2, 2, true)]
    [InlineData(3, 2, false)]
    [InlineData(1, 1, true)]
    [InlineData(1, 2, true)]
    public async Task GetMettrics_ReviewerThreshold_BehavesCorrectly(int minReviewers, int uniqueReviewers, bool thresholdReached)
    {
        // Arrange
        var options = CreateOptions(minReviewers: minReviewers);
        var searchResponse = CreateSearchResponse(CreatePRNode(123, "author"));
        var reviews = new List<ReviewNode>();

        for (int i = 0; i < uniqueReviewers; i++)
        {
            reviews.Add(CreateReview($"user{i}", i + 1));
        }

        var detailsResponse = CreateDetailsResponse(reviews: reviews.ToArray());
        var mockClient = CreateMockClient(searchResponse, detailsResponse);
        var service = CreateService(options, mockClient.Object);

        // Act
        var result = await service.GetMetricsAsync(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31));

        // Assert
        Assert.True(result.IsSuccess);
        var metrics = result.Value!.Single();

        if (thresholdReached)
            Assert.NotNull(metrics.MinimumReviewersReachedAt);
        else
            Assert.Null(metrics.MinimumReviewersReachedAt);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public async Task GetMetricsAsync_SameReviewerMultipleTimes_CountsOnce(
    int reviewCount)
    {
        // Arrange
        var options = CreateOptions(minReviewers: 2);

        var searchResponse = CreateSearchResponse(
            CreatePRNode(123, "author")
        );

        var reviews = Enumerable.Range(1, reviewCount)
            .Select(i => CreateReview("alice", i))
            .ToArray();

        var detailsResponse = CreateDetailsResponse(
            reviews: reviews
        );

        var mockClient = CreateMockClient(searchResponse, detailsResponse);

        var service = CreateService(options, mockClient.Object);

        // Act
        var result = await service.GetMetricsAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31));

        // Assert
        Assert.True(result.IsSuccess);
        var metrics = result.Value!.Single();

        Assert.Equal(1, metrics.TotalReviewersCount);
    }


    // ---------- approval tests ----------

    [Theory]
    [InlineData(2, 2, true)]
    [InlineData(3, 2, false)]
    [InlineData(1, 1, true)]
    public async Task GetMetricsAsync_ApprovalThreshold_BehavesCorrectly(
    int minimumApprovals,
    int approvals,
    bool thresholdReached)
    {
        var options = CreateOptions(minApprovals: minimumApprovals);

        var searchResponse = CreateSearchResponse(
            CreatePRNode(123, "author")
        );

        var reviews = Enumerable.Range(1, approvals)
            .Select(i => CreateReview($"user{i}", i, "APPROVED"))
            .ToArray();

        var detailsResponse = CreateDetailsResponse(
            reviews: reviews
        );

        var mockClient = CreateMockClient(searchResponse, detailsResponse);

        var service = CreateService(options, mockClient.Object);

        var result = await service.GetMetricsAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31));

        var metrics = result.Value!.Single();

        if (thresholdReached)
            Assert.NotNull(metrics.MinimumApprovalsReachedAt);
        else
            Assert.Null(metrics.MinimumApprovalsReachedAt);
    }


    [Fact]
    public async Task GetMetricsAsync_OnlyCommentsNoApprovals_CountsZeroApprovals()
    {
        // Arrange
        var options = CreateOptions(minApprovals: 1);
        var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var searchResponse = CreateSearchResponse(
            CreatePRNode(123, "author")
        );

        var detailsResponse = CreateDetailsResponse(
            createdAt: baseTime,
            reviews: new[]
            {
                CreateReview("alice", 1, "COMMENTED"),
                CreateReview("bob", 2, "CHANGES_REQUESTED")
            }
        );

        var mockClient = CreateMockClient(searchResponse, detailsResponse);
        var service = CreateService(options, mockClient.Object);

        // Act
        var result = await service.GetMetricsAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31));

        // Assert
        Assert.True(result.IsSuccess);
        var metrics = result.Value!.Single();
        Assert.Equal(0, metrics.TotalApprovalsCount);
        Assert.Null(metrics.FirstApprovalAt);
        Assert.Null(metrics.MinimumApprovalsReachedAt);
    }

    // ---------- ordering tests ----------

    [Fact]
    public async Task GetMetricsAsync_FirstReview_IsEarliestReview()
    {
        // Arrange
        var options = CreateOptions();
        var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var searchResponse = CreateSearchResponse(
            CreatePRNode(123, "author")
        );

        var detailsResponse = CreateDetailsResponse(
            createdAt: baseTime,
            reviews: new[]
            {
                CreateReview("bob", 5),
                CreateReview("alice", 2),
                CreateReview("charlie", 8)
            }
        );

        var mockClient = CreateMockClient(searchResponse, detailsResponse);
        var service = CreateService(options, mockClient.Object);

        // Act
        var result = await service.GetMetricsAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31));

        // Assert
        Assert.True(result.IsSuccess);
        var metrics = result.Value!.Single();
        Assert.Equal(baseTime.AddHours(2), metrics.FirstReviewAt);
    }

    [Fact]
    public async Task GetMetricsAsync_FirstApproval_IsEarliestApproval()
    {
        // Arrange
        var options = CreateOptions();
        var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var searchResponse = CreateSearchResponse(
            CreatePRNode(123, "author")
        );

        var detailsResponse = CreateDetailsResponse(
            createdAt: baseTime,
            reviews: new[]
            {
                CreateReview("alice", 1, "COMMENTED"),
                CreateReview("bob", 5, "APPROVED"),
                CreateReview("charlie", 3, "APPROVED")
            }
        );

        var mockClient = CreateMockClient(searchResponse, detailsResponse);
        var service = CreateService(options, mockClient.Object);

        // Act
        var result = await service.GetMetricsAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31));

        // Assert
        Assert.True(result.IsSuccess);
        var metrics = result.Value!.Single();
        Assert.Equal(baseTime.AddHours(3), metrics.FirstApprovalAt);
    }

    // ---------- null handling ----------

    [Fact]
    public async Task GetMetricsAsync_NoReviews_ReturnsNullForAllReviewMetrics()
    {
        // Arrange
        var options = CreateOptions();
        var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var searchResponse = CreateSearchResponse(
            CreatePRNode(123, "author")
        );

        var detailsResponse = CreateDetailsResponse(
            createdAt: baseTime,
            reviews: Array.Empty<ReviewNode>()
        );

        var mockClient = CreateMockClient(searchResponse, detailsResponse);
        var service = CreateService(options, mockClient.Object);

        // Act
        var result = await service.GetMetricsAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31));

        // Assert
        Assert.True(result.IsSuccess);
        var metrics = result.Value!.Single();
        Assert.Equal(0, metrics.TotalReviewersCount);
        Assert.Null(metrics.FirstReviewAt);
        Assert.Null(metrics.MinimumReviewersReachedAt);
        Assert.Equal(0, metrics.TotalApprovalsCount);
        Assert.Null(metrics.FirstApprovalAt);
        Assert.Null(metrics.MinimumApprovalsReachedAt);
    }

    // ---------- multiple PRs ----------

    [Fact]
    public async Task GetMetricsAsync_WithMultiplePRs_ReturnsMetricsForAll()
    {
        // Arrange
        var options = CreateOptions(minReviewers: 1, minApprovals: 1);
        var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var searchResponse = CreateSearchResponse(
            CreatePRNode(123, "alice"),
            CreatePRNode(456, "bob"),
            CreatePRNode(789, "charlie")
        );

        var detailsResponse = CreateDetailsResponse(
            createdAt: baseTime,
            reviews: new[] { CreateReview("reviewer", 1, "APPROVED") }
        );

        var mockClient = CreateMockClient(searchResponse, detailsResponse);
        var service = CreateService(options, mockClient.Object);

        // Act
        var result = await service.GetMetricsAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31));

        // Assert
        Assert.True(result.IsSuccess);
        var metrics = result.Value!.ToList();
        Assert.Equal(3, metrics.Count);
        Assert.Contains(metrics, m => m.PullRequestNumber == 123);
        Assert.Contains(metrics, m => m.PullRequestNumber == 456);
        Assert.Contains(metrics, m => m.PullRequestNumber == 789);
    }

    // ---------- error handling ----------

    [Fact]
    public async Task GetMetricsAsync_WhenSearchFails_ReturnsFailure()
    {
        // Arrange
        var options = CreateOptions();
        var mockClient = new Mock<IGitHubClient>();

        mockClient.Setup(c => c.ExecuteQueryAsync<SearchResponseData>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(Result<SearchResponseData>.Failure("GraphQL error"));

        var service = CreateService(options, mockClient.Object);

        // Act
        var result = await service.GetMetricsAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31));

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("GraphQL error", result.Error);
    }

    [Fact]
    public async Task GetMetricsAsync_WhenDetailsFails_SkipsThatPR()
    {
        // Arrange
        var options = CreateOptions();

        var searchResponse = CreateSearchResponse(
            CreatePRNode(123, "alice"),
            CreatePRNode(456, "bob")
        );

        var mockClient = new Mock<IGitHubClient>();

        mockClient.Setup(c => c.ExecuteQueryAsync<SearchResponseData>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(Result<SearchResponseData>.Success(searchResponse));

        var callCount = 0;
        mockClient.Setup(c => c.ExecuteQueryAsync<PullRequestDetailsResponseData>(
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                    return Result<PullRequestDetailsResponseData>.Failure("PR not found");

                return Result<PullRequestDetailsResponseData>.Success(
                    CreateDetailsResponse(reviews: Array.Empty<ReviewNode>()));
            });

        var service = CreateService(options, mockClient.Object);

        // Act
        var result = await service.GetMetricsAsync(
            new DateTime(2025, 1, 1),
            new DateTime(2025, 1, 31));

        // Assert
        Assert.True(result.IsSuccess);
        var metrics = result.Value!.ToList();
        Assert.Single(metrics);
        Assert.Equal(456, metrics[0].PullRequestNumber);
    }
}
