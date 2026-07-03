using AiCodeReviewAgent.Application.Repositories;

namespace AiCodeReviewAgent.Application.Reports;

public interface IMarkdownReportService
{
    string Generate(RepositoryAnalysisResponse analysis);
}

public sealed class MarkdownReportService : IMarkdownReportService
{
    public string Generate(RepositoryAnalysisResponse analysis)
    {
        var report = new System.Text.StringBuilder();

        report.AppendLine("# AI Code Review Report");
        report.AppendLine();
        report.AppendLine($"**Repository:** `{analysis.RepositoryPath}`");
        report.AppendLine($"**Files analyzed:** {analysis.FilesAnalyzed}");
        report.AppendLine($"**Total findings:** {analysis.TotalFindings}");
        report.AppendLine();
        report.AppendLine("## Findings by Severity");
        report.AppendLine();
        report.AppendLine($"- High: {analysis.HighFindings}");
        report.AppendLine($"- Medium: {analysis.MediumFindings}");
        report.AppendLine($"- Low: {analysis.LowFindings}");
        report.AppendLine();

        foreach (var file in analysis.Files.Where(x => x.Analysis.Findings.Any()))
        {
            report.AppendLine($"## {file.FilePath}");
            report.AppendLine();

            foreach (var finding in file.Analysis.Findings)
            {
                report.AppendLine($"- **{finding.Severity}** line {finding.Line}: {finding.Message}");
            }

            report.AppendLine();
        }

        return report.ToString();
    }
}