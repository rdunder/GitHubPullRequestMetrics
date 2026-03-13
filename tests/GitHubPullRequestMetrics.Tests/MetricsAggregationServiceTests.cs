using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.Models.Metrics;
using GitHubPullRequestMetrics.Services;

namespace GitHubPullRequestMetrics.Tests;

public class MetricsAggregationServiceTests
{
    // ---------- helpers ----------

    private static MetricsAggregationService CreateService() =>
        new(new GitHubOptions
        {
            Token = "token",
            DefaultOwner = "owner",
            DefaultRepository = "repo",
            MinimumReviewers = 2,
            MinimumApprovals = 2
        });

    private static PullRequestMetricsDto CreateMetric(
        double? firstReview = null,
        double? minReviewers = null,
        double? firstApproval = null,
        double? minApprovals = null,
        double? merge = null)
    {
        var created = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        return new PullRequestMetricsDto
        {
            PullRequestNumber = 1,
            Author = "author",
            CreatedAt = created,
            FirstReviewAt = firstReview.HasValue ? created.AddHours(firstReview.Value) : null,
            MinimumReviewersReachedAt = minReviewers.HasValue ? created.AddHours(minReviewers.Value) : null,
            FirstApprovalAt = firstApproval.HasValue ? created.AddHours(firstApproval.Value) : null,
            MinimumApprovalsReachedAt = minApprovals.HasValue ? created.AddHours(minApprovals.Value) : null,
            MergedAt = merge.HasValue ? created.AddHours(merge.Value) : null
        };
    }

    private static MetricsSummaryDto Aggregate(IEnumerable<PullRequestMetricsDto> metrics)
        => CreateService().AggregateMetrics(metrics);

    // ---------- metric definitions ----------

    private static readonly (
        Func<double?, PullRequestMetricsDto> CreateMetric,
        Func<MetricsSummaryDto, TimeSpan?> Average,
        Func<MetricsSummaryDto, TimeSpan?> Median
    )[] MetricCases =
    {
        (
            v => CreateMetric(firstReview: v),
            s => s.AverageTimeToFirstReview,
            s => s.MedianTimeToFirstReview
        ),
        (
            v => CreateMetric(minReviewers: v),
            s => s.AverageTimeToMinimumReviewers,
            s => s.MedianTimeToMinimumReviewers
        ),
        (
            v => CreateMetric(firstApproval: v),
            s => s.AverageTimeToFirstApproval,
            s => s.MedianTimeToFirstApproval
        ),
        (
            v => CreateMetric(minApprovals: v),
            s => s.AverageTimeToMinimumApprovals,
            s => s.MedianTimeToMinimumApprovals
        ),
        (
            v => CreateMetric(merge: v),
            s => s.AverageTimeToMerge,
            s => s.MedianTimeToMerge
        )
    };

    public static IEnumerable<object[]> GetMetricCases()
    {
        foreach (var m in MetricCases)
            yield return new object[] { m.CreateMetric, m.Average, m.Median };
    }

    // ---------- generic metric tests ----------

    [Theory]
    [MemberData(nameof(GetMetricCases))]
    public void Average_IsCalculatedCorrectly(
        Func<double?, PullRequestMetricsDto> createMetric,
        Func<MetricsSummaryDto, TimeSpan?> averageSelector,
        Func<MetricsSummaryDto, TimeSpan?> _)
    {
        var metrics = new[]
        {
            createMetric(2),
            createMetric(4),
            createMetric(6)
        };

        var summary = Aggregate(metrics);

        Assert.Equal(TimeSpan.FromHours(4), averageSelector(summary));
    }

    [Theory]
    [MemberData(nameof(GetMetricCases))]
    public void Median_IsCalculatedCorrectly(
        Func<double?, PullRequestMetricsDto> createMetric,
        Func<MetricsSummaryDto, TimeSpan?> _,
        Func<MetricsSummaryDto, TimeSpan?> medianSelector)
    {
        var metrics = new[]
        {
            createMetric(2),
            createMetric(4),
            createMetric(6),
            createMetric(8)
        };

        var summary = Aggregate(metrics);

        Assert.Equal(TimeSpan.FromHours(5), medianSelector(summary));
    }

    [Theory]
    [MemberData(nameof(GetMetricCases))]
    public void Metrics_IgnoreNullValues(
        Func<double?, PullRequestMetricsDto> createMetric,
        Func<MetricsSummaryDto, TimeSpan?> averageSelector,
        Func<MetricsSummaryDto, TimeSpan?> medianSelector)
    {
        var metrics = new[]
        {
            createMetric(2),
            createMetric(null),
            createMetric(6)
        };

        var summary = Aggregate(metrics);

        Assert.Equal(TimeSpan.FromHours(4), averageSelector(summary));
        Assert.Equal(TimeSpan.FromHours(4), medianSelector(summary));
    }

    [Theory]
    [MemberData(nameof(GetMetricCases))]
    public void Metrics_WithSingleValue_ReturnThatValue(
        Func<double?, PullRequestMetricsDto> createMetric,
        Func<MetricsSummaryDto, TimeSpan?> averageSelector,
        Func<MetricsSummaryDto, TimeSpan?> medianSelector)
    {
        var metrics = new[]
        {
            createMetric(5)
        };

        var summary = Aggregate(metrics);

        Assert.Equal(TimeSpan.FromHours(5), averageSelector(summary));
        Assert.Equal(TimeSpan.FromHours(5), medianSelector(summary));
    }

    // ---------- counts ----------

    [Fact]
    public void Counts_AreCalculatedCorrectly()
    {
        var baseTime = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var metrics = new List<PullRequestMetricsDto>
        {
            new()
            {
                CreatedAt = baseTime,
                FirstReviewAt = baseTime.AddHours(1),
                MinimumReviewersReachedAt = baseTime.AddHours(2),
                FirstApprovalAt = baseTime.AddHours(3),
                MinimumApprovalsReachedAt = baseTime.AddHours(4)
            },
            new()
            {
                CreatedAt = baseTime,
                FirstReviewAt = baseTime.AddHours(1),
                FirstApprovalAt = baseTime.AddHours(2)
            },
            new()
            {
                CreatedAt = baseTime
            }
        };

        var summary = Aggregate(metrics);

        Assert.Equal(3, summary.TotalPRs);
        Assert.Equal(2, summary.PRsWithReviews);
        Assert.Equal(1, summary.PRsWithMinimumReviewers);
        Assert.Equal(2, summary.PRsWithApprovals);
        Assert.Equal(1, summary.PRsWithMinimumApprovals);
    }

    // ---------- edge cases ----------

    [Fact]
    public void AggregateMetrics_WithEmptyList_ReturnsEmptySummary()
    {
        var summary = Aggregate([]);

        Assert.Equal(0, summary.TotalPRs);
        Assert.Empty(summary.PullRequests);

        Assert.Null(summary.AverageTimeToFirstReview);
        Assert.Null(summary.MedianTimeToFirstReview);
    }

    [Fact]
    public void PullRequests_IsReadOnly()
    {
        var metrics = new[] { CreateMetric(firstReview: 1) };
        var summary = Aggregate(metrics);

        Assert.IsAssignableFrom<IReadOnlyList<PullRequestMetricsDto>>(summary.PullRequests);
    }

    [Theory]
    [MemberData(nameof(GetMetricCases))]
    public void Median_WorksWithUnsortedValues(
    Func<double?, PullRequestMetricsDto> createMetric,
    Func<MetricsSummaryDto, TimeSpan?> _,
    Func<MetricsSummaryDto, TimeSpan?> medianSelector)
    {
        var metrics = new[]
        {
        createMetric(10),
        createMetric(1),
        createMetric(5)
    };

        var summary = Aggregate(metrics);

        Assert.Equal(TimeSpan.FromHours(5), medianSelector(summary));
    }

    [Theory]
    [MemberData(nameof(GetMetricCases))]
    public void Average_ReturnsFractionalTimes(
    Func<double?, PullRequestMetricsDto> createMetric,
    Func<MetricsSummaryDto, TimeSpan?> averageSelector,
    Func<MetricsSummaryDto, TimeSpan?> _)
    {
        var metrics = new[]
        {
        createMetric(1),
        createMetric(2)
    };

        var summary = Aggregate(metrics);

        Assert.Equal(TimeSpan.FromHours(1.5), averageSelector(summary));
    }

    [Theory]
    [MemberData(nameof(GetMetricCases))]
    public void Metrics_AllNull_ReturnsNull(
    Func<double?, PullRequestMetricsDto> createMetric,
    Func<MetricsSummaryDto, TimeSpan?> averageSelector,
    Func<MetricsSummaryDto, TimeSpan?> medianSelector)
    {
        var metrics = new[]
        {
        createMetric(null),
        createMetric(null)
    };

        var summary = Aggregate(metrics);

        Assert.Null(averageSelector(summary));
        Assert.Null(medianSelector(summary));
    }
}