using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.Models.Metrics;
using Spectre.Console;

namespace GitHubPullRequestMetrics.Cli.Services;

internal class TableOutputFormatter(GitHubOptions options) : IOutputFormatter
{
    public void DisplaySummary(MetricsSummaryDto summary)
    {
        AnsiConsole.MarkupLine("[bold cyan]📊 METRICS SUMMARY[/]");
        AnsiConsole.WriteLine();

        var overviewTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Cyan1)
            .AddColumn(new TableColumn("[bold]Metric[/]").LeftAligned())
            .AddColumn(new TableColumn("[bold]Count[/]").RightAligned());

        overviewTable.AddRow("Total PRs", summary.TotalPRs.ToString());
        overviewTable.AddRow("PRs with reviews", summary.PRsWithReviews.ToString());
        overviewTable.AddRow("PRs with minimum reviewers", summary.PRsWithMinimumReviewers.ToString());
        overviewTable.AddRow("PRs with approvals", summary.PRsWithApprovals.ToString());
        overviewTable.AddRow("PRs with minimum approvals", summary.PRsWithMinimumApprovals.ToString());

        AnsiConsole.Write(overviewTable);
        AnsiConsole.WriteLine();

        var timeTable = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .AddColumn(new TableColumn("[bold]Time Metric[/]").LeftAligned())
            .AddColumn(new TableColumn("[bold]Average[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Median[/]").RightAligned());

        AddTimeRow(timeTable, "Time to first review",
            summary.AverageTimeToFirstReview,
            summary.MedianTimeToFirstReview);

        AddTimeRow(timeTable, "Time to minimum reviewers",
            summary.AverageTimeToMinimumReviewers,
            summary.MedianTimeToMinimumReviewers);

        AddTimeRow(timeTable, "Time to first approval",
            summary.AverageTimeToFirstApproval,
            summary.MedianTimeToFirstApproval);

        AddTimeRow(timeTable, "Time to minimum approvals",
            summary.AverageTimeToMinimumApprovals,
            summary.MedianTimeToMinimumApprovals);

        AddTimeRow(timeTable, "Time to merge",
            summary.AverageTimeToMerge,
            summary.MedianTimeToMerge);

        AnsiConsole.Write(timeTable);
    }

    public void DisplayIndividualMetrics(IEnumerable<PullRequestMetricsDto> metrics)
    {
        metrics = metrics.ToList();
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]📋 INDIVIDUAL PR METRICS[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow)
            .AddColumn(new TableColumn("[bold]PR #[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Title[/]"))
            .AddColumn(new TableColumn("[bold]Time to 1st Review[/]").RightAligned())
            .AddColumn(new TableColumn("[bold]Time to Merge[/]").RightAligned());
        
        

        foreach (var metric in metrics.OrderByDescending(m => m.TimeToMerge))
        {
            table.AddRow(
                new Text($"# {metric.PullRequestNumber}"),
                new Markup(FormatPrLink(metric)),
                new Markup(FormatTimeSpan(metric.TimeToFirstReview)),
                new Markup(FormatTimeSpan(metric.TimeToMerge))
            );
        }

        AnsiConsole.Write(table);
    }

    private static void AddTimeRow(Table table, string metric, TimeSpan? average, TimeSpan? median)
    {
        table.AddRow(
            metric,
            FormatTimeSpan(average),
            FormatTimeSpan(median)
        );
    }

    private string FormatPrLink(PullRequestMetricsDto metric)
    {
        var title = Markup.Escape(metric.Title);
        
        if (title.Length > 35)
            title = title[..35] + "...";

        return
            $"[blue underline link=https://github.com/" +
            $"{options.Owner}/{options.Repository}/pull/" +
            $"{metric.PullRequestNumber}]{title}[/]";
    }

    private static string FormatTimeSpan(TimeSpan? timeSpan)
    {
        if (!timeSpan.HasValue)
            return "[dim]N/A[/]";

        var ts = timeSpan.Value;

        if (ts.TotalDays >= 1)
            return $"[yellow]{ts.TotalDays:F1}d[/]";

        if (ts.TotalHours >= 1)
            return $"[cyan]{ts.TotalHours:F1}h[/]";

        return $"[green]{ts.TotalMinutes:F0}m[/]";
    }

    private static string EscapeMarkup(string text)
    {
        return text.Replace("[", "[[").Replace("]", "]]");
    }
}
