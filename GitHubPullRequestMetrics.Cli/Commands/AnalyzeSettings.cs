using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitHubPullRequestMetrics.Cli.Commands;

/// <summary>
/// CLI settings for the analyze command.
/// Supports date ranges via --from/--to or --days.
/// </summary>
public class AnalyzeSettings : CommandSettings
{
    [CommandOption("--from")]
    [Description("Start date (YYYY-MM-DD)")]
    public DateTime? From { get; set; }

    [CommandOption("--to")]
    [Description("End date (YYYY-MM-DD)")]
    public DateTime? To { get; set; }

    [CommandOption("--days")]
    [Description("Number of days back from today (default: 30)")]
    public int? Days { get; set; }

    [CommandOption("--owner")]
    [Description("Repository owner (overrides appsettings.json)")]
    public string? Owner { get; set; }

    [CommandOption("--repo")]
    [Description("Repository name (overrides appsettings.json)")]
    public string? Repository { get; set; }

    [CommandOption("--show-individual")]
    [Description("Show individual PR details (not just summary)")]
    [DefaultValue(false)]
    public bool ShowIndividual { get; set; }

    /// <summary>
    /// Validates that CLI arguments are consistent.
    /// </summary>
    public override ValidationResult Validate()
    {
        if (Days.HasValue && (From.HasValue || To.HasValue))
        {
            return ValidationResult.Error("Cannot use --days together with --from/--to");
        }

        if ((From.HasValue && !To.HasValue) || (!From.HasValue && To.HasValue))
        {
            return ValidationResult.Error("Both --from and --to must be specified together");
        }

        if (Days.HasValue && Days.Value <= 0)
        {
            return ValidationResult.Error("--days must be a positive number");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Calculates the date range based on provided options.
    /// Defaults to last 30 days if nothing specified.
    /// </summary>
    public (DateTime From, DateTime To) GetDateRange()
    {
        if (Days.HasValue)
        {
            var to = DateTime.Now;
            var from = to.AddDays(-Days.Value);
            return (from, to);
        }

        if (From.HasValue && To.HasValue)
        {
            return (From.Value, To.Value);
        }

        var defaultTo = DateTime.Now;
        var defaultFrom = defaultTo.AddDays(-30);
        return (defaultFrom, defaultTo);
    }
}
