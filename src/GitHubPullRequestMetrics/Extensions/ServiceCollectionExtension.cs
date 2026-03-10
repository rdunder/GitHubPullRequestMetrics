using GitHubPullRequestMetrics.Configuration;
using GitHubPullRequestMetrics.Interfaces;
using GitHubPullRequestMetrics.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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
        services.AddSingleton<IPullrequestMetricsService, PullrequestMetricsService>();

        return services;
    }
}
