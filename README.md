# GitHub Pull Request Metrics

A .NET library for fetching and analyzing Pull Request metrics from GitHub repositories. Built to help teams gain insights into their PR review process during sprint planning and retrospectives.

## 📊 Features

- **Fetch PR metrics** for any GitHub repository within a date range
- **Calculate key metrics:**
  - Time to first review
  - Time to approval
  - Time to merge
  - Review turnaround time
- **Filter by team members** to focus on specific contributors
- **Flexible integration** - use as a library in CLI, API, or desktop applications
- **Built with .NET 10** and modern best practices

## 🚀 Quick Start

### Prerequisites

- .NET 10 SDK
- GitHub Personal Access Token (PAT) with permissions:
  - Pull Requests: Read
  - Contents: Read

### Installation
```bash
# Clone the repository
git clone https://github.com/yourusername/GithubPullRequestMetrics.git
cd GithubPullRequestMetrics

# Build the solution
dotnet build

# Run the CLI example
cd src/GithubPullRequestMetrics.Cli
dotnet run
```

### Creating a GitHub Token

1. Go to [GitHub Settings → Tokens](https://github.com/settings/tokens?type=beta)
2. Click "Generate new token (fine-grained)"
3. Set permissions:
   - Repository access: Public Repositories (or specific repos)
   - Pull requests: Read-only
   - Contents: Read-only
4. Copy the generated token

## 📖 Usage

### CLI Application

Configure `appsettings.json`:
```json
{
  "GitHub": {
    "Token": "ghp_your_token_here",
    "DefaultOwner": "dotnet",
    "DefaultRepository": "runtime",
    "TeamMembers": ["alice", "bob", "charlie"]
  }
}
```

Run:
```bash
dotnet run
```

### As a Library

**1. Add NuGet reference:**
```bash
dotnet add reference path/to/GithubPullRequestMetrics.Core
```

**2. Register services:**
```csharp
using GithubPullRequestMetrics.Core.Extensions;

builder.Services.AddGitHubPullRequestMetrics(options =>
{
    options.Token = "ghp_your_token";
    options.DefaultOwner = "my-org";
    options.DefaultRepository = "my-repo";
    options.TeamMembers = new List<string> { "alice", "bob" };
});
```

**3. Use the service:**
```csharp
using GithubPullRequestMetrics.Core.Interfaces;

var result = await metricsService.GetMetricsAsync(
    from: DateTime.Now.AddMonths(-1),
    to: DateTime.Now
);

if (result.IsSuccess)
{
    foreach (var metric in result.Value)
    {
        Console.WriteLine($"PR #{metric.PullRequestNumber} by {metric.Author}");
        Console.WriteLine($"  Time to merge: {metric.TimeToMerge?.TotalDays:F1} days");
    }
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
```

## 🏗️ Architecture
```
GithubPullRequestMetrics/
├─ Models/
│   ├─ GraphQL/          # GitHub API response models
│   └─ Metrics/          # Calculated metrics DTOs
├─ Services/
│   ├─ GitHubGraphQLClient.cs
│   └─ PullRequestMetricsService.cs
├─ Interfaces/
│   └─ IPullRequestMetricsService.cs
│   └─ IGitHubClient.cs
├─ Configuration/
│   └─ GitHubOptions.cs
└─ Extensions/
    └─ ServiceCollectionExtensions.cs
```

## 📦 NuGet Packages Used

- `GraphQL.Client` - GraphQL client for .NET
- `GraphQL.Client.Serializer.SystemTextJson` - JSON serialization
- `Microsoft.Extensions.DependencyInjection` - Dependency injection
- `Microsoft.Extensions.Http` - HttpClientFactory support

## 🔧 Configuration Options

| Option | Description | Required |
|--------|-------------|----------|
| `Token` | GitHub Personal Access Token | ✅ Yes |
| `DefaultOwner` | Repository owner/organization | ✅ Yes |
| `DefaultRepository` | Repository name | ✅ Yes |
| `TeamMembers` | List of GitHub usernames to filter | ❌ No |

## 📊 Metrics Provided

Each `PullRequestMetricsDto` contains:

- `PullRequestNumber` - PR identifier
- `Author` - GitHub username of PR creator
- `CreatedAt` - When the PR was created
- `FirstReviewAt` - Timestamp of first review
- `ApprovedAt` - When the PR was approved
- `MergedAt` - When the PR was merged
- `TimeToFirstReview` - Duration from creation to first review
- `TimeToApproval` - Duration from creation to approval
- `TimeToMerge` - Total duration from creation to merge
- `ReviewToApprovalTime` - Duration from first review to approval

## 🧪 Testing

Run tests:
```bash
dotnet test
```

## 🛣️ Roadmap

- [ ] Add unit tests
- [ ] Support for multiple review cycles
- [ ] Export metrics to CSV/JSON
- [ ] Dashboard visualization
- [ ] Support for GitLab and Azure DevOps
- [ ] Caching layer for faster repeated queries

## 📝 Example Output
```
Fetching PR metrics from 2025-02-10 to 2025-03-10...

Found 47 merged PRs

====================================================================================================

PR #12345 by @johndoe
  Created:          2025-02-15 10:30
  First Review:     2025-02-15 14:20 (3.8 hours)
  Approved:         2025-02-16 09:15 (22.8 hours)
  Merged:           2025-02-16 10:00 (23.5 hours)

====================================================================================================

📊 SUMMARY
Total PRs:                47
PRs with reviews:         45
PRs approved:             43

Avg time to first review: 4.2 hours
Avg time to merge:        1.3 days
```

## 🤝 Contributing

This project is part of an educational thesis work. Contributions and feedback are welcome!

## 📄 License

MIT License - see LICENSE file for details

## 👨‍💻 Author

Created as part of a thesis project at EC Utbildning, Gothenburg

## 🙏 Acknowledgments

- GitHub GraphQL API documentation
- .NET community for excellent tools and libraries
- [Any other acknowledgments]

---

**Built with ❤️ using .NET 10**