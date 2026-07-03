namespace AiCodeReviewAgent.Application.Repositories;

public sealed class PullRequestReviewResponse
{
    public int PullRequestNumber { get; set; }
    public int FilesReviewed { get; set; }
    public string ReviewMarkdown { get; set; } = string.Empty;
}