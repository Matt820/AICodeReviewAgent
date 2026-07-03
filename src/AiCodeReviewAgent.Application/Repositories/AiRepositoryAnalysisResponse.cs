namespace AiCodeReviewAgent.Application.Repositories;

public sealed class AiRepositoryAnalysisResponse
{
    public string RepositoryPath { get; set; } = string.Empty;
    public int FilesAnalyzed { get; set; }
    public List<AiFileAnalysisResult> Files { get; set; } = [];
}

public sealed class AiFileAnalysisResult
{
    public string FilePath { get; set; } = string.Empty;
    public string AiReview { get; set; } = string.Empty;
}