namespace AiCodeReviewAgent.Application.Repositories;

public sealed class PullRequestAnalysisRequest
{
    public string Repository { get; set; } = string.Empty;
    public int PullRequestNumber { get; set; }
    public string GitHubToken { get; set; } = string.Empty;
    public int MaxFiles { get; set; } = 10;
}