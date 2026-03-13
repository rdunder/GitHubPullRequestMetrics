using GitHubPullRequestMetrics.Cli.Commands;
using GitHubPullRequestMetrics.Cli.Services;
using GitHubPullRequestMetrics.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.DependencyInjection;

namespace GithubPullRequestMetrics.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddGitHubPullRequestMetrics(options =>
        {
            builder.Configuration.GetSection("GitHub").Bind(options);
        });

        builder.Services.AddSingleton<IOutputFormatter, TableOutputFormatter>();

        var registrar = new DependencyInjectionRegistrar(builder.Services);

        var app = new CommandApp(registrar);

        app.Configure(config =>
        {
            config.SetApplicationName("pr-metrics");

            config.AddCommand<AnalyzeCommand>("analyze");

            config.AddExample("analyze", "--days", "7");
            config.AddExample("analyze", "--from", "2026-01-01", "--to", "2026-01-31");
            config.AddExample("analyze", "--days", "14", "--show-individual");
        });

        return await app.RunAsync(args);
    }
}