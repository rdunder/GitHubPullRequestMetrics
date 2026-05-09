using GitHubPullRequestMetrics.Extensions;
using GitHubPullRequestMetrics.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddGitHubPullRequestMetrics(options =>
{
    options.Token = Environment.GetEnvironmentVariable("GITHUB__TOKEN")
        ?? throw new InvalidOperationException("env var GITHUB__TOKEN not found.");
    options.Owner = "dotnet";
    options.Repository = "runtime";
    options.MinimumReviewers = 2;
    options.MinimumApprovals = 2;
});

var app = builder.Build();
var service = app.Services.GetRequiredService<IPullRequestMetricsService>();

var result = await service.GetPullRequestMetricsAsync(
    DateTime.Now.AddDays(-3),
    DateTime.Now
);

if (result.IsSuccess)
{
    var summary = result.Value;
    Console.WriteLine($"Total PRs: {summary.TotalPRs}");
    Console.WriteLine($"Avg time to merge: {summary.AverageTimeToMerge}");
    Console.WriteLine($"Median time to merge: {summary.MedianTimeToMerge}");

    foreach (var pr in summary.PullRequests)
    {
        Console.WriteLine($"PR #{pr.PullRequestNumber} - {pr.Title} by {pr.Author}");
        Console.WriteLine($"  Time to first review:   {pr.TimeToFirstReview?.TotalHours:F1}h");
        Console.WriteLine($"  Time to first approval: {pr.TimeToFirstApproval?.TotalHours:F1}h");
        Console.WriteLine($"  Time to merge:          {pr.TimeToMerge?.TotalHours:F1}h");
    }
}