using GitHubPullRequestMetrics.Extensions;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// Load configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Register GitHub metrics services
builder.Services.AddGitHubPullRequestMetrics(options =>
{
    builder.Configuration.GetSection("GitHub").Bind(options);
});

var app = builder.Build();

// Get the service
var metricsService = app.Services.GetRequiredService<IPullRequestMetricsService>();

// Set date range (last month)
var to = DateTime.Now;
var from = to.AddMonths(-1);

Console.WriteLine($"Fetching PR metrics from {from:yyyy-MM-dd} to {to:yyyy-MM-dd}...\n");

// Fetch metrics
var result = await metricsService.GetMetricsAsync(from, to);

if (!result.IsSuccess)
{
    Console.WriteLine($"❌ Error: {result.Error}");
    return 1;
}

var metrics = result.Value!.ToList();

if (metrics.Count == 0)
{
    Console.WriteLine("No merged PRs found in the specified date range.");
    return 0;
}

Console.WriteLine($"Found {metrics.Count} merged PRs\n");
Console.WriteLine("=".PadRight(100, '='));

foreach (var metric in metrics.OrderBy(m => m.PullRequestNumber))
{
    Console.WriteLine($"\nPR #{metric.PullRequestNumber} by @{metric.Author}");
    Console.WriteLine($"  Created:          {metric.CreatedAt:yyyy-MM-dd HH:mm}");

    if (metric.FirstReviewAt.HasValue)
    {
        Console.WriteLine($"  First Review:     {metric.FirstReviewAt:yyyy-MM-dd HH:mm} " +
                         $"({FormatTimeSpan(metric.TimeToFirstReview)})");
    }
    else
    {
        Console.WriteLine($"  First Review:     No reviews");
    }

    if (metric.ApprovedAt.HasValue)
    {
        Console.WriteLine($"  Approved:         {metric.ApprovedAt:yyyy-MM-dd HH:mm} " +
                         $"({FormatTimeSpan(metric.TimeToApproval)})");
    }
    else
    {
        Console.WriteLine($"  Approved:         Not approved");
    }

    if (metric.MergedAt.HasValue)
    {
        Console.WriteLine($"  Merged:           {metric.MergedAt:yyyy-MM-dd HH:mm} " +
                         $"({FormatTimeSpan(metric.TimeToMerge)})");
    }
}

// Summary statistics
Console.WriteLine("\n" + "=".PadRight(100, '='));
Console.WriteLine("\n📊 SUMMARY");
Console.WriteLine($"Total PRs:                {metrics.Count}");
Console.WriteLine($"PRs with reviews:         {metrics.Count(m => m.FirstReviewAt.HasValue)}");
Console.WriteLine($"PRs approved:             {metrics.Count(m => m.ApprovedAt.HasValue)}");

var avgTimeToFirstReview = metrics
    .Where(m => m.TimeToFirstReview.HasValue)
    .Select(m => m.TimeToFirstReview!.Value.TotalHours)
    .DefaultIfEmpty(0)
    .Average();

var avgTimeToMerge = metrics
    .Where(m => m.TimeToMerge.HasValue)
    .Select(m => m.TimeToMerge!.Value.TotalHours)
    .DefaultIfEmpty(0)
    .Average();

Console.WriteLine($"\nAvg time to first review: {FormatHours(avgTimeToFirstReview)}");
Console.WriteLine($"Avg time to merge:        {FormatHours(avgTimeToMerge)}");

return 0;

// Helper function to format TimeSpan
static string FormatTimeSpan(TimeSpan? timeSpan)
{
    if (!timeSpan.HasValue) return "N/A";

    var ts = timeSpan.Value;
    if (ts.TotalDays >= 1)
        return $"{ts.TotalDays:F1} days";
    if (ts.TotalHours >= 1)
        return $"{ts.TotalHours:F1} hours";
    return $"{ts.TotalMinutes:F0} minutes";
}

// Helper function to format average hours
static string FormatHours(double hours)
{
    if (hours >= 24)
        return $"{hours / 24:F1} days";
    if (hours >= 1)
        return $"{hours:F1} hours";
    return $"{hours * 60:F0} minutes";
}