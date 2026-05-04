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
    DateTime.Now.AddDays(-2),
    DateTime.Now
);

if (!result.IsSuccess)
{
    Console.WriteLine($"Fel: {result.Error}");
    return;
}

var summary = result.Value;


Console.WriteLine("=== Individual Pull Requests ===");
Console.WriteLine();

foreach (var pr in summary.PullRequests)
{
    Console.WriteLine($"PR #{pr.PullRequestNumber} author: {pr.Author}");
    Console.WriteLine($"  created:                  {pr.CreatedAt:yyyy-MM-dd HH:mm}");
    Console.WriteLine($"  Merged:                   {(pr.MergedAt.HasValue ? pr.MergedAt.Value.ToString("yyyy-MM-dd HH:mm") : "ej mergad")}");
    Console.WriteLine($"  Time to first review:     {FormatTimeSpan(pr.TimeToFirstReview)}");
    Console.WriteLine($"  Time to min. reviewers:   {FormatTimeSpan(pr.TimeToMinimumReviewers)} ({pr.TotalReviewersCount} reviewers)");
    Console.WriteLine($"  Time to first approval:   {FormatTimeSpan(pr.TimeToFirstApproval)}");
    Console.WriteLine($"  Time to min. approvals:   {FormatTimeSpan(pr.TimeToMinimumApprovals)} ({pr.TotalApprovalsCount} approvals)");
    Console.WriteLine($"  Time to merge:            {FormatTimeSpan(pr.TimeToMerge)}");
    Console.WriteLine();
}

// --- Aggregerad sammanfattning ---
Console.WriteLine("=== Summary ===");
Console.WriteLine($"Total PRs:          {summary.TotalPRs}");
Console.WriteLine($"with reviews:           {summary.PRsWithReviews}");
Console.WriteLine($"with min. reviewers:    {summary.PRsWithMinimumReviewers}");
Console.WriteLine($"with approvals:         {summary.PRsWithApprovals}");
Console.WriteLine($"with min. approvals:    {summary.PRsWithMinimumApprovals}");
Console.WriteLine();
Console.WriteLine($"average time to merge:  {FormatTimeSpan(summary.AverageTimeToMerge)}");
Console.WriteLine($"Median time to merge:   {FormatTimeSpan(summary.MedianTimeToMerge)}");

static string FormatTimeSpan(TimeSpan? ts) =>
    ts.HasValue
        ? $"{(int)ts.Value.TotalHours}h {ts.Value.Minutes}m"
        : "n/a";