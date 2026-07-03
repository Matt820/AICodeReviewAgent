namespace AiCodeReviewAgent.Application.Reports;

public sealed class SaveMarkdownReportRequest
{
    public string RepositoryPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
}