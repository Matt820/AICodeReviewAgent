using AiCodeReviewAgent.Application.Repositories;
using Microsoft.AspNetCore.Mvc;
using AiCodeReviewAgent.Application.Reports;

namespace AiCodeReviewAgent.Api.Controllers;

[ApiController]
[Route("api/repositories")]
public sealed class RepositoriesController : ControllerBase
{
    private readonly IRepositoryAnalysisService _repositoryAnalysisService;    
    private readonly IMarkdownReportService _markdownReportService;
    private readonly IAiRepositoryAnalysisService _aiRepositoryAnalysisService;
    private readonly IAiMarkdownReportService _aiMarkdownReportService;

    public RepositoriesController(
        IRepositoryAnalysisService repositoryAnalysisService,
        IMarkdownReportService markdownReportService,
        IAiRepositoryAnalysisService aiRepositoryAnalysisService,
        IAiMarkdownReportService aiMarkdownReportService
        )
    {
        _repositoryAnalysisService = repositoryAnalysisService;
        _markdownReportService = markdownReportService;
        _aiRepositoryAnalysisService = aiRepositoryAnalysisService;
        _aiMarkdownReportService = aiMarkdownReportService;
    }

    [HttpPost("analyze-local")]
    public async Task<ActionResult<RepositoryAnalysisResponse>> AnalyzeLocal(
        [FromBody] AnalyzeLocalRepositoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _repositoryAnalysisService.AnalyzeLocalAsync(request, cancellationToken);
        return Ok(result);
    }
    [HttpPost("analyze-local/markdown")]
    public async Task<ActionResult<MarkdownReportResponse>> AnalyzeLocalMarkdown(
        [FromBody] AnalyzeLocalRepositoryRequest request,
        CancellationToken cancellationToken)
    {
        var analysis = await _repositoryAnalysisService.AnalyzeLocalAsync(request, cancellationToken);
        var markdown = _markdownReportService.Generate(analysis);

        return Ok(new MarkdownReportResponse
        {
            Markdown = markdown
        });
    }
    [HttpPost("analyze-local/markdown/save")]
    public async Task<ActionResult> AnalyzeLocalMarkdownAndSave(
        [FromBody] SaveMarkdownReportRequest request,
        CancellationToken cancellationToken)
    {
        var analysis = await _repositoryAnalysisService.AnalyzeLocalAsync(
            new AnalyzeLocalRepositoryRequest
            {
                RepositoryPath = request.RepositoryPath
            },
            cancellationToken);

        var markdown = _markdownReportService.Generate(analysis);

        var outputPath = string.IsNullOrWhiteSpace(request.OutputPath)
            ? Path.Combine(request.RepositoryPath, "code-review-report.md")
            : request.OutputPath;

        await System.IO.File.WriteAllTextAsync(outputPath, markdown, cancellationToken);

        return Ok(new
        {
            message = "Reporte generado correctamente.",
            outputPath
        });
    }
    [HttpPost("analyze-local/ai-markdown/save")]
    public async Task<ActionResult<object>> AnalyzeLocalAiMarkdownAndSave(
        [FromBody] SaveMarkdownReportRequest request,
        CancellationToken cancellationToken)
    {
        var analysis = await _aiRepositoryAnalysisService.AnalyzeLocalWithAiAsync(
            new AnalyzeLocalRepositoryRequest
            {
                RepositoryPath = request.RepositoryPath,
                MaxFiles = 3,
                UseAi = true
            },
            cancellationToken);

        var markdown = _aiMarkdownReportService.Generate(analysis);

        var outputPath = string.IsNullOrWhiteSpace(request.OutputPath)
            ? Path.Combine(request.RepositoryPath, "code-review-ai-report.md")
            : request.OutputPath;

        await System.IO.File.WriteAllTextAsync(outputPath, markdown, cancellationToken);

        return Ok(new
        {
            message = "Reporte IA generado correctamente.",
            filesAnalyzed = analysis.FilesAnalyzed,
            outputPath
        });
    }
}