using AiCodeReviewAgent.Application.Repositories;
using AiCodeReviewAgent.Application.Reports;
using AiCodeReviewAgent.Application.Reviews;
using AiCodeReviewAgent.Infrastructure.Ai;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using AiCodeReviewAgent.Infrastructure.GitHub;
using AiCodeReviewAgent.Application.Agents;
using AiCodeReviewAgent.Application.Tools;
using AiCodeReviewAgent.Application.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddHttpClient<IAiCodeReviewClient, OpenAiCodeReviewClient>();
builder.Services.AddHttpClient<IGitHubPullRequestClient, GitHubPullRequestClient>();
builder.Services.AddHttpClient<IGitHubPullRequestCommentClient, GitHubPullRequestCommentClient>();
builder.Services.AddHttpClient<IGitHubPullRequestCommentManager, GitHubPullRequestCommentManager>();

builder.Services.AddScoped<IAiRepositoryAnalysisService, AiRepositoryAnalysisService>();
builder.Services.AddScoped<IAiMarkdownReportService, AiMarkdownReportService>();
builder.Services.AddScoped<IAgentTool, ReadFileTool>();
builder.Services.AddScoped<IAgentTool, SearchTextTool>();
builder.Services.AddScoped<ICodeReviewAgent, CodeReviewAgent>();
builder.Services.AddScoped<IAgentTool, RunBuildTool>();
builder.Services.AddScoped<IAgentTool, RunTestsTool>();
builder.Services.AddScoped<IAiReviewConfigurationLoader, AiReviewConfigurationLoader>();


using var host = builder.Build();

if (args.Length >= 1 && args[0] == "analyze-pr")
{
    var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
    var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

    if (string.IsNullOrWhiteSpace(repository))
        throw new InvalidOperationException("No se encontró GITHUB_REPOSITORY.");

    if (string.IsNullOrWhiteSpace(githubToken))
        throw new InvalidOperationException("No se encontró GITHUB_TOKEN.");

    var prNumber = GitHubEventReader.GetPullRequestNumber();

    using var prScope = host.Services.CreateScope();

    var configLoader = prScope.ServiceProvider.GetRequiredService<IAiReviewConfigurationLoader>();

    var workspacePath = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")
        ?? Directory.GetCurrentDirectory();

    var config = await configLoader.LoadAsync(workspacePath, CancellationToken.None);

    var githubClient = prScope.ServiceProvider.GetRequiredService<IGitHubPullRequestClient>();

    Console.WriteLine($"Analizando PR #{prNumber} en {repository}...");

    var files = await githubClient.GetChangedFilesAsync(
        new PullRequestAnalysisRequest
        {
            Repository = repository,
            PullRequestNumber = prNumber,
            GitHubToken = githubToken,
            MaxFiles = config.MaxFiles,
        },
        CancellationToken.None);

    Console.WriteLine($"Archivos .cs modificados encontrados: {files.Count}");

    var codeReviewAgent = prScope.ServiceProvider.GetRequiredService<ICodeReviewAgent>();
    //var commentClient = prScope.ServiceProvider.GetRequiredService<IGitHubPullRequestCommentClient>();
    var commentManager = prScope.ServiceProvider.GetRequiredService<IGitHubPullRequestCommentManager>();

    var prMarkdown = new System.Text.StringBuilder();

    var tools = prScope.ServiceProvider.GetServices<IAgentTool>();

    var buildTool = tools.FirstOrDefault(x => x.Name == "run_build");
    var testTool = tools.FirstOrDefault(x => x.Name == "run_tests");

    /* var workspacePath = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")
        ?? Directory.GetCurrentDirectory(); */

    var buildResult = buildTool is null
        ? null
        : await buildTool.ExecuteAsync(workspacePath, string.Empty, CancellationToken.None);

    var testResult = testTool is null
        ? null
        : await testTool.ExecuteAsync(workspacePath, string.Empty, CancellationToken.None);

    var reviewScore = ReviewScoreCalculator.Calculate(
        buildResult,
        testResult,
        files.Count);

    if (files.Count == 0)
    {
        //var commentManager = prScope.ServiceProvider.GetRequiredService<IGitHubPullRequestCommentManager>();

        await commentManager.UpsertCommentAsync(
            repository,
            prNumber,
            githubToken,
            """
            ## 🤖 AI Code Review

            No se encontraron archivos `.cs` modificados en este Pull Request.

            ✅ No se ejecutó análisis con IA para evitar consumo innecesario.
            """,
            CancellationToken.None);

        Console.WriteLine("No hay archivos .cs modificados. Se publicó comentario informativo.");
        return;
    }
    
    

    prMarkdown.AppendLine("## 🤖 AI Code Review");
    prMarkdown.AppendLine();
    prMarkdown.AppendLine($"Pull Request: #{prNumber}");
    prMarkdown.AppendLine($"Archivos `.cs` analizados: {files.Count}");
    prMarkdown.AppendLine();

    prMarkdown.AppendLine("### Estado del PR");
    prMarkdown.AppendLine();

    prMarkdown.AppendLine($"**Review Score:** {reviewScore}/100");
    prMarkdown.AppendLine();

    prMarkdown.AppendLine($"- Build: {(buildResult?.Success == true ? "✅ Passed" : "❌ Failed")}");
    prMarkdown.AppendLine($"- Tests: {(testResult?.Success == true ? "✅ Passed" : "❌ Failed")}");
    prMarkdown.AppendLine();

    foreach (var file in files)
    { 
        if (string.IsNullOrWhiteSpace(file.Patch))
            continue;
        
            var review = await codeReviewAgent.ReviewPullRequestAsync(
                Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? Directory.GetCurrentDirectory(),
                file.FileName,
                file.Patch,
                buildResult,
                testResult,
                config.Rules,
                CancellationToken.None);

        prMarkdown.AppendLine($"### `{file.FileName}`");
        prMarkdown.AppendLine();
        prMarkdown.AppendLine(review);
        prMarkdown.AppendLine();
        prMarkdown.AppendLine("---");
        prMarkdown.AppendLine();
    }

    await commentManager.UpsertCommentAsync(
        repository,
        prNumber,
        githubToken,
        prMarkdown.ToString(),
        CancellationToken.None);

    Console.WriteLine("Comentario publicado en el Pull Request.");

    foreach (var file in files)
    {
        Console.WriteLine($"- {file.FileName} ({file.Status})");
    }

    return;
}

if (args.Length < 2 || args[0] != "analyze")
{
    Console.WriteLine("Uso:");
    Console.WriteLine("  dotnet run --project src/AiCodeReviewAgent.Cli -- analyze \"RUTA_REPOSITORIO\"");
    return;
}

var repositoryPath = args[1];

var outputPath = args.Length >= 3
    ? args[2]
    : Path.Combine(repositoryPath, "code-review-ai-report.md");

using var scope = host.Services.CreateScope();

var analyzer = scope.ServiceProvider.GetRequiredService<IAiRepositoryAnalysisService>();
var reportService = scope.ServiceProvider.GetRequiredService<IAiMarkdownReportService>();

Console.WriteLine("Analizando repositorio con IA...");

var result = await analyzer.AnalyzeLocalWithAiAsync(
    new AnalyzeLocalRepositoryRequest
    {
        RepositoryPath = repositoryPath,
        MaxFiles = 3,
        UseAi = true
    },
    CancellationToken.None);

var markdown = reportService.Generate(result);

await File.WriteAllTextAsync(outputPath, markdown);

Console.WriteLine($"Reporte generado: {outputPath}");
