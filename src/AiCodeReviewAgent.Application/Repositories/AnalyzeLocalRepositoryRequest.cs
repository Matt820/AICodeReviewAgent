namespace AiCodeReviewAgent.Application.Repositories;

public sealed class AnalyzeLocalRepositoryRequest
{
    public string RepositoryPath { get; set; } = string.Empty;
    public bool UseAi { get; set; } = false;
    public int MaxFiles { get; set; } = 3;
}