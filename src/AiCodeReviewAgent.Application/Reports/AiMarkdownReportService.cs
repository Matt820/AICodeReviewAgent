using System.Text;
using AiCodeReviewAgent.Application.Repositories;

namespace AiCodeReviewAgent.Application.Reports;

public interface IAiMarkdownReportService
{
    string Generate(AiRepositoryAnalysisResponse analysis);
}

public sealed class AiMarkdownReportService : IAiMarkdownReportService
{
    public string Generate(AiRepositoryAnalysisResponse analysis)
    {
        var report = new StringBuilder();

        report.AppendLine("# AI Code Review Report");
        report.AppendLine();
        report.AppendLine($"**Repository:** `{analysis.RepositoryPath}`");
        report.AppendLine($"**Files analyzed:** {analysis.FilesAnalyzed}");
        report.AppendLine();

        foreach (var file in analysis.Files)
        {
            report.AppendLine($"## {file.FilePath}");
            report.AppendLine();
            report.AppendLine(file.AiReview);
            report.AppendLine();
            report.AppendLine("---");
            report.AppendLine();
        }

        return report.ToString();
    }
}