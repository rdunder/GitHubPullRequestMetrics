using GitHubPullRequestMetrics.Cli.Services;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Models.GraphQL.Common;
using GitHubPullRequestMetrics.Models.Metrics;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubPullRequestMetrics.Cli.Commands;

internal class AnalyzeCommand : AsyncCommand<AnalyzeSettings>
{
    private readonly IPullRequestMetricsService _metricsService;
    private readonly IOutputFormatter _outputFormatter;

    public AnalyzeCommand(
        IPullRequestMetricsService metricsService,
        IOutputFormatter outputFormatter)
    {
        _metricsService = metricsService;
        _outputFormatter = outputFormatter;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, AnalyzeSettings settings, CancellationToken cancellationToken)
    {
        var (from, to) = settings.GetDateRange();

        DisplayHeader(from, to);

        var result = await FetchMetricsWithProgress(from, to, settings.Owner, settings.Repository);

        if (!result.IsSuccess)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error:[/] {result.Error}");
            return 1;
        }

        var summary = result.Value!;

        if (summary.TotalPRs == 0)
        {
            AnsiConsole.MarkupLine("[yellow]⚠️  No merged PRs found in the specified date range.[/]");
            return 0;
        }

        _outputFormatter.DisplaySummary(summary);

        if (settings.ShowIndividual)
        {
            _outputFormatter.DisplayIndividualMetrics(summary.PullRequests);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]✓ Analyzed {summary.TotalPRs} PRs successfully[/]");

        return 0;
    }

    private static void DisplayHeader(DateTime from, DateTime to)
    {
        var rule = new Rule($"[bold cyan]Analyzing PRs from {from:yyyy-MM-dd} to {to:yyyy-MM-dd}[/]")
        {
            Justification = Justify.Left,
            Style = Style.Parse("cyan")
        };

        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    private async Task<Result<MetricsSummaryDto>> FetchMetricsWithProgress(
        DateTime from,
        DateTime to,
        string? owner,
        string? repository)
    {
        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync("Fetching PR metrics from GitHub...", async ctx =>
            {
                ctx.Status("Searching for merged PRs...");
                await Task.Delay(100);

                var result = await _metricsService.GetMetricsSummaryAsync(
                    from,
                    to,
                    owner,
                    repository);

                if (result.IsSuccess)
                {
                    ctx.Status($"Processing {result.Value!.TotalPRs} PRs...");
                    await Task.Delay(100);
                }

                return result;
            });
    }

    
}
