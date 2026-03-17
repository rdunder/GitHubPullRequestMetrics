using GraphQL;

namespace GitHubPullRequestMetrics.GraphQL.Helpers;

public static class QueryBuilder
{
    public static GraphQLRequest GetPullRequestSearchQuery(
        string searchQuery, 
        string? cursor = null) =>
            new GraphQLRequest
            {
                Query = @"
                    query ($query: String!, $cursor: String) {
                        search(type: ISSUE, query: $query, first: 100, after: $cursor) {
                            nodes {
                                ... on PullRequest {
                                    number
                                    title
                                    createdAt
                                    mergedAt
                                    author {
                                        login
                                    }
                                }
                            }
                            pageInfo {
                                hasNextPage
                                endCursor
                            }
                        }
                    }",
                Variables = new
                {
                    query = searchQuery,
                    cursor
                }
            };

    
    public static GraphQLRequest GetPullRequestDetailsQuery(
        string owner,
        string repo,
        int number) => new GraphQLRequest
        {
            Query = @"
                query ($owner: String!, $repo: String!, $number: Int!) {
                    repository(owner: $owner, name: $repo) {
                        pullRequest(number: $number) {
                            title
                            createdAt
                            mergedAt
                            reviews(first: 100) {
                                nodes {
                                    state
                                    submittedAt
                                    author {
                                        login
                                    }
                                }
                            }
                        }
                    }
                }",
            Variables = new
            {
                owner,
                repo,
                number
            }
        };
}