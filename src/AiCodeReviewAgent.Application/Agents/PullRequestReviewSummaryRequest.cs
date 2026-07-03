namespace AiCodeReviewAgent.Application.Agents;

public sealed class PullRequestReviewSummaryRequest
{
    public int PullRequestNumber { get; set; }
    public int FilesReviewed { get; set; }
    public int ReviewScore { get; set; }
    public bool BuildPassed { get; set; }
    public bool TestsPassed { get; set; }
    public string ReviewsMarkdown { get; set; } = string.Empty;
}