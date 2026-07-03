namespace AiCodeReviewAgent.Application.Repositories;

public sealed class PullRequestChangedFile
{
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Patch { get; set; } = string.Empty;
}