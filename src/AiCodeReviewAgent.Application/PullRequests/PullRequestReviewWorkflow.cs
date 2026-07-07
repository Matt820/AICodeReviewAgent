using System.Text;
using AiCodeReviewAgent.Application.Agents;
using AiCodeReviewAgent.Application.Configuration;
using AiCodeReviewAgent.Application.Repositories;
using AiCodeReviewAgent.Application.Reviews;
using AiCodeReviewAgent.Application.Tools;

namespace AiCodeReviewAgent.Application.PullRequests;

public sealed class PullRequestReviewWorkflow : IPullRequestReviewWorkflow
{
    private readonly IAiReviewConfigurationLoader _configLoader;
    private readonly IGitHubPullRequestClient _githubClient;
    private readonly IGitHubPullRequestCommentManager _commentManager;
    private readonly ICodeReviewAgent _codeReviewAgent;
    private readonly IPullRequestSummaryAgent _summaryAgent;
    private readonly IEnumerable<IAgentTool> _tools;

    public PullRequestReviewWorkflow(
        IAiReviewConfigurationLoader configLoader,
        IGitHubPullRequestClient githubClient,
        IGitHubPullRequestCommentManager commentManager,
        ICodeReviewAgent codeReviewAgent,
        IPullRequestSummaryAgent summaryAgent,
        IEnumerable<IAgentTool> tools)
    {
        _configLoader = configLoader;
        _githubClient = githubClient;
        _commentManager = commentManager;
        _codeReviewAgent = codeReviewAgent;
        _summaryAgent = summaryAgent;
        _tools = tools;
    }

    public async Task ExecuteAsync(
        PullRequestReviewWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var config = await _configLoader.LoadAsync(
            request.WorkspacePath,
            cancellationToken);

        Console.WriteLine($"Analizando PR #{request.PullRequestNumber} en {request.Repository}...");

        var files = await _githubClient.GetChangedFilesAsync(
            new PullRequestAnalysisRequest
            {
                Repository = request.Repository,
                PullRequestNumber = request.PullRequestNumber,
                GitHubToken = request.GitHubToken,
                MaxFiles = config.MaxFiles
            },
            cancellationToken);

        Console.WriteLine($"Archivos .cs modificados encontrados: {files.Count}");

        var buildResult = await ExecuteToolIfAvailableAsync(
            "run_build",
            request.WorkspacePath,
            string.Empty,
            cancellationToken);

        var testResult = await ExecuteToolIfAvailableAsync(
            "run_tests",
            request.WorkspacePath,
            string.Empty,
            cancellationToken);

        var reviewScore = ReviewScoreCalculator.Calculate(
            buildResult,
            testResult,
            files.Count);

        if (files.Count == 0)
        {
            await _commentManager.UpsertCommentAsync(
                request.Repository,
                request.PullRequestNumber,
                request.GitHubToken,
                """
                ## 🤖 AI Code Review

                No se encontraron archivos `.cs` modificados en este Pull Request.

                ✅ No se ejecutó análisis con IA para evitar consumo innecesario.
                """,
                cancellationToken);

            Console.WriteLine("No hay archivos .cs modificados. Se publicó comentario informativo.");
            return;
        }

        var prMarkdown = new StringBuilder();
        var reviewsMarkdown = new StringBuilder();

        AppendHeader(
            prMarkdown,
            request.PullRequestNumber,
            files.Count,
            config.Language,
            config.MaxFiles,
            config.Rules,
            reviewScore,
            buildResult,
            testResult);

        foreach (var file in files)
        {
            if (string.IsNullOrWhiteSpace(file.Patch))
                continue;

            var review = await _codeReviewAgent.ReviewPullRequestAsync(
                request.WorkspacePath,
                file.FileName,
                file.Patch,
                buildResult,
                testResult,
                config.Rules,
                config.Features,
                cancellationToken);

            reviewsMarkdown.AppendLine($"### `{file.FileName}`");
            reviewsMarkdown.AppendLine();
            reviewsMarkdown.AppendLine(review);
            reviewsMarkdown.AppendLine();

            prMarkdown.AppendLine($"### `{file.FileName}`");
            prMarkdown.AppendLine();
            prMarkdown.AppendLine(review);
            prMarkdown.AppendLine();
            prMarkdown.AppendLine("---");
            prMarkdown.AppendLine();
        }

        var executiveSummary = await _summaryAgent.GenerateSummaryAsync(
            new PullRequestReviewSummaryRequest
            {
                PullRequestNumber = request.PullRequestNumber,
                FilesReviewed = files.Count,
                ReviewScore = reviewScore,
                BuildPassed = buildResult?.Success == true,
                TestsPassed = testResult?.Success == true,
                ReviewsMarkdown = reviewsMarkdown.ToString()
            },
            cancellationToken);

        prMarkdown.AppendLine("## Resumen ejecutivo");
        prMarkdown.AppendLine();
        prMarkdown.AppendLine(executiveSummary);
        prMarkdown.AppendLine();
        prMarkdown.AppendLine("---");
        prMarkdown.AppendLine();

        await _commentManager.UpsertCommentAsync(
            request.Repository,
            request.PullRequestNumber,
            request.GitHubToken,
            prMarkdown.ToString(),
            cancellationToken);

        Console.WriteLine("Comentario publicado en el Pull Request.");

        foreach (var file in files)
        {
            Console.WriteLine($"- {file.FileName} ({file.Status})");
        }
    }

    private async Task<AgentToolResult?> ExecuteToolIfAvailableAsync(
        string toolName,
        string repositoryPath,
        string input,
        CancellationToken cancellationToken)
    {
        var tool = _tools.FirstOrDefault(x => x.Name == toolName);

        if (tool is null)
            return null;

        return await tool.ExecuteAsync(
            repositoryPath,
            input,
            cancellationToken);
    }

    private static void AppendHeader(
        StringBuilder prMarkdown,
        int pullRequestNumber,
        int filesCount,
        string language,
        int maxFiles,
        IReadOnlyList<string> rules,
        int reviewScore,
        AgentToolResult? buildResult,
        AgentToolResult? testResult)
    {
        prMarkdown.AppendLine("## 🤖 AI Code Review");
        prMarkdown.AppendLine();
        prMarkdown.AppendLine($"Pull Request: #{pullRequestNumber}");
        prMarkdown.AppendLine($"Archivos `.cs` analizados: {filesCount}");
        prMarkdown.AppendLine();

        prMarkdown.AppendLine("### Configuración del agente");
        prMarkdown.AppendLine();
        prMarkdown.AppendLine($"- Lenguaje: `{language}`");
        prMarkdown.AppendLine($"- Máximo de archivos: `{maxFiles}`");

        if (rules.Count > 0)
        {
            prMarkdown.AppendLine("- Reglas activas:");

            foreach (var rule in rules)
            {
                prMarkdown.AppendLine($"  - {rule}");
            }
        }

        prMarkdown.AppendLine();

        prMarkdown.AppendLine("### Estado del PR");
        prMarkdown.AppendLine();
        prMarkdown.AppendLine($"**Review Score:** {reviewScore}/100");
        prMarkdown.AppendLine();
        prMarkdown.AppendLine($"- Build: {(buildResult?.Success == true ? "✅ Passed" : "❌ Failed")}");
        prMarkdown.AppendLine($"- Tests: {(testResult?.Success == true ? "✅ Passed" : "❌ Failed")}");
        prMarkdown.AppendLine();
    }
}