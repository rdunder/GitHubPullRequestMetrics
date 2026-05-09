# GitHub Pull Request Metrics

A .NET library and CLI tool for analyzing Pull Request metrics from GitHub repositories.

## Features

- 📊 **Comprehensive metrics**: Time to first review, approvals, and merge
- 📈 **Statistical analysis**: Both average and median values
- 👥 **Team filtering**: Track specific team members
- ⚙️ **Configurable thresholds**: Minimum reviewers and approvals
- 🎨 **Beautiful CLI**: Colored tables with Spectre.Console
- 📦 **Reusable library**: Integrate into your own tools

## Quick Start

### Prerequisites

- .NET 10 SDK
- GitHub Personal Access Token with:
  - Pull Requests: Read
  - Contents: Read

[Create token here](https://github.com/settings/tokens?type=beta)

### Installation
```bash
git clone https://github.com/rdunder/GithubPullRequestMetrics.git
cd GithubPullRequestMetrics
```

### Configuration
Add your github token as an enviroment variable: GITHUB__TOKEN

Create `GithubPullRequestMetrics.Cli/appsettings.json`:
```json
{
  "GitHub": {
    "Token": "The safest way is to add token as enviroment variable, but it can be added here as well",
    "Owner": "owner",
    "Repository": "repo",
    "TeamMembers": ["alice", "bob"],
    "MinimumReviewers": 2,
    "MinimumApprovals": 2
  }
}
```

### Run CLI
If you create executable from CLI you run with ```pr-metrics``` 

```bash
cd GithubPullRequestMetrics.Cli

# See all commands and arguments
dotnet run

# Last 30 days (default)
dotnet run -- analyze

# Last 7 days
dotnet run -- analyze --days 7

# Specific date range
dotnet run -- analyze --from 2026-02-01 --to 2026-02-28

# With individual PR details table (sorted by time-to-merge, with clickable GitHub links)
dotnet run -- analyze --days 14 --show-individual
```

## Using as a Library

### Install

Add a reference to the library project:
```bash
dotnet add reference path/to/GitHubPullRequestMetrics/GitHubPullRequestMetrics.csproj
```

Your host project also needs:
```bash
dotnet add package Microsoft.Extensions.Hosting
```

### Example
```csharp
using GitHubPullRequestMetrics.Extensions;
using GitHubPullRequestMetrics.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddGitHubPullRequestMetrics(options =>
{
    options.Token = "SAFETY FIRST - Enviroment Variable";
    options.Owner = "my-org";
    options.Repository = "my-repo";
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
```

## Metrics Provided

| Metric | Description |
|--------|-------------|
| **Time to First Review** | Duration from PR creation to first review |
| **Time to Minimum Reviewers** | Duration until required number of reviewers |
| **Time to First Approval** | Duration from creation to first approval |
| **Time to Minimum Approvals** | Duration until required number of approvals |
| **Time to Merge** | Total duration from creation to merge |

All metrics include both **average** and **median** values.

## Project Structure
```
GithubPullRequestMetrics/
├─ src/
│   └─ GithubPullRequestMetrics/        # Reusable library
├─ GithubPullRequestMetrics.Cli/        # CLI tool (Spectre.Console)
├─ MinimalExampleCli/                   # Minimal example of using the library directly
└─ tests/
    └─ GithubPullRequestMetricsTests/
```

## Library Structure
```
src/GitHubPullRequestMetrics/
├─ Configuration/
│   └─ GitHubOptions.cs                 # Token, Owner, Repository, thresholds
├─ Extensions/
│   └─ ServiceCollectionExtension.cs    # AddGitHubPullRequestMetrics() DI registration
├─ GraphQL/
│   ├─ Helpers/QueryBuilder.cs          # Builds search and detail GraphQL requests
│   └─ Models/                          # GraphQL response deserialization types
├─ Interfaces/
│   ├─ IGitHubClient.cs
│   ├─ IMetricsAggregationService.cs
│   └─ IPullRequestMetricsService.cs    # Primary public interface for consumers
├─ Models/
│   ├─ PullRequestMetricsDto.cs         # Per-PR data with calculated TimeSpan properties
│   ├─ MetricsSummaryDto.cs             # Aggregated averages, medians, and PR list
│   └─ Result.cs                        # Result<T> for error propagation without exceptions
└─ Services/
    ├─ GitHubGraphQLClient.cs           # HTTP + GraphQL execution
    ├─ MetricsAggregationService.cs     # Computes averages and medians across PRs
    └─ PullRequestMetricsService.cs     # Orchestrates fetch + metric calculation
```

## Configuration Options

| Option | Required | Description |
|--------|----------|-------------|
| `Token` | ✅ | GitHub Personal Access Token |
| `Owner` | ✅ | Repository owner/organization |
| `Repository` | ✅ | Repository name |
| `TeamMembers` | ❌ | Filter PRs by these GitHub usernames |
| `MinimumReviewers` | ❌ | Required unique reviewers (default: 1) |
| `MinimumApprovals` | ❌ | Required approvals (default: 1) |


## Technology Stack

- .NET 10
- GraphQL.Client
- Spectre.Console
- xUnit

## License

MIT

## Author

Created as part of a thesis project.