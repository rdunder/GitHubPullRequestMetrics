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
git clone https://github.com/yourusername/GithubPullRequestMetrics.git
cd GithubPullRequestMetrics
```

### Configuration
Add your github token as an enviroment variable: GITHUB__TOKEN

Create `GithubPullRequestMetrics.Cli/appsettings.json`:
```json
{
  "GitHub": {
    "Token": "The safest way is to add token as enviroment variable, but it can be added here as well",
    "DefaultOwner": "owner",
    "DefaultRepository": "repo",
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

# With individual PR details
dotnet run -- analyze --days 14 --show-individual
```

## Using as a Library

### Install
```bash
dotnet add reference GithubPullRequestMetrics/GithubPullRequestMetrics
```

### Example
```csharp
using GithubPullRequestMetrics.Core.Extensions;
using GithubPullRequestMetrics.Core.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddGitHubPullRequestMetrics(options =>
{
    options.Token = "SAFETY FIRST - Enviroment Variable";
    options.DefaultOwner = "my-org";
    options.DefaultRepository = "my-repo";
    options.MinimumReviewers = 2;
    options.MinimumApprovals = 2;
});

var app = builder.Build();
var service = app.Services.GetRequiredService();

var result = await service.GetMetricsSummaryAsync(
    DateTime.Now.AddMonths(-1),
    DateTime.Now
);

if (result.IsSuccess)
{
    var summary = result.Value;
    Console.WriteLine($"Total PRs: {summary.TotalPRs}");
    Console.WriteLine($"Avg time to merge: {summary.AverageTimeToMerge}");
    Console.WriteLine($"Median time to merge: {summary.MedianTimeToMerge}");
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
├─ GithubPullRequestMetrics.Cli/        # CLI tool
└─ tests/
    └─ GithubPullRequestMetricsTests/
```

## Configuration Options

| Option | Required | Description |
|--------|----------|-------------|
| `Token` | ✅ | GitHub Personal Access Token |
| `DefaultOwner` | ✅ | Repository owner/organization |
| `DefaultRepository` | ✅ | Repository name |
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