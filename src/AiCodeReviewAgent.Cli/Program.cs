using AiCodeReviewAgent.Application.Repositories;
using AiCodeReviewAgent.Application.Reports;
using AiCodeReviewAgent.Application.Reviews;
using AiCodeReviewAgent.Infrastructure.Ai;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddHttpClient<IAiCodeReviewClient, OpenAiCodeReviewClient>();
builder.Services.AddScoped<IAiRepositoryAnalysisService, AiRepositoryAnalysisService>();
builder.Services.AddScoped<IAiMarkdownReportService, AiMarkdownReportService>();

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

    var githubClient = prScope.ServiceProvider.GetRequiredService<IGitHubPullRequestClient>();

    Console.WriteLine($"Analizando PR #{prNumber} en {repository}...");

    var files = await githubClient.GetChangedFilesAsync(
        new PullRequestAnalysisRequest
        {
            Repository = repository,
            PullRequestNumber = prNumber,
            GitHubToken = githubToken,
            MaxFiles = 10
        },
        CancellationToken.None);

    Console.WriteLine($"Archivos .cs modificados encontrados: {files.Count}");

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
