using AiCodeReviewAgent.Application.Reviews;

namespace AiCodeReviewAgent.Application.Repositories;

public sealed class RepositoryAnalysisResponse
{
    public string RepositoryPath { get; set; } = string.Empty;
    public int FilesAnalyzed { get; set; }
    public int TotalFindings { get; set; }
    public List<FileAnalysisResult> Files { get; set; } = [];
    public int HighFindings { get; set; }
    public int MediumFindings { get; set; }
    public int LowFindings { get; set; }
}

public sealed class FileAnalysisResult
{
    public string FilePath { get; set; } = string.Empty;
    public AnalyzeCodeResponse Analysis { get; set; } = new();
}