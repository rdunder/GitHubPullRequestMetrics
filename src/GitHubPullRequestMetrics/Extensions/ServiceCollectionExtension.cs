using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubPullRequestMetrics.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGitHubPullRequestMetrics(
        this IServiceCollection services,
        Action<GitHubOptions> configureOptions)
    {
        var options = new GitHubOptions();
        configureOptions(options);
        options.Validate();
        services.AddSingleton(options);

        services.AddHttpClient("GitHub");

        services.AddSingleton<IGitHubClient, GitHubGraphQLClient>();
        services.AddSingleton<IMetricsAggregationService, MetricsAggregationService>();
        services.AddSingleton<IPullRequestMetricsService, PullRequestMetricsService>();

        return services;
    }
}
