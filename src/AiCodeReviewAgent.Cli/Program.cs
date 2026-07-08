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
using AiCodeReviewAgent.Application.Agents.Orchestration;
using AiCodeReviewAgent.Application.Agents.Planning;
using AiCodeReviewAgent.Application.Agents.Specialized;
using AiCodeReviewAgent.Application.Agents.Tools;
using AiCodeReviewAgent.Application.PullRequests;
using AiCodeReviewAgent.Application.Agents.Execution;
using AiCodeReviewAgent.Application.Agents.Prompts;
using AiCodeReviewAgent.Application.Rag;
using AiCodeReviewAgent.Infrastructure.Rag;
using AiCodeReviewAgent.Application.Observability;
using AiCodeReviewAgent.Application.Agents.Pipeline;
using AiCodeReviewAgent.Application.Agents.Pipeline.Stages;


var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

//builder.Services.AddHttpClient<IAiCodeReviewClient, OpenAiCodeReviewClient>();

builder.Services.AddScoped<AiUsageMetrics>();

builder.Services.AddHttpClient<OpenAiCodeReviewClient>();

builder.Services.AddScoped<IAiCodeReviewClient>(sp =>
{
    var inner = sp.GetRequiredService<OpenAiCodeReviewClient>();
    var metrics = sp.GetRequiredService<AiUsageMetrics>();
    var budgetGuard = sp.GetRequiredService<IAiBudgetGuard>();
    return new MeteredAiCodeReviewClient(inner, metrics, budgetGuard);
});

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
builder.Services.AddScoped<IPullRequestSummaryAgent, PullRequestSummaryAgent>();
//builder.Services.AddScoped<IAgentPlanner, HeuristicAgentPlanner>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();
builder.Services.AddScoped<IAgentToolRegistry, AgentToolRegistry>();
builder.Services.AddScoped<IPullRequestReviewWorkflow, PullRequestReviewWorkflow>();
builder.Services.AddScoped<IAgentToolExecutor, AgentToolExecutor>();
builder.Services.AddScoped<ICodeReviewPromptBuilder, CodeReviewPromptBuilder>();
builder.Services.AddScoped<HeuristicAgentPlanner>();
builder.Services.AddScoped<IAgentPlanner, LlmAgentPlanner>();
builder.Services.AddScoped<IAgentTool, ReadSolutionTool>();
builder.Services.AddScoped<IAgentTool, ReadProjectFileTool>();
builder.Services.AddScoped<IAgentTool, FindClassTool>();
builder.Services.AddScoped<IAgentTool, FindInterfaceTool>();
builder.Services.AddScoped<IAgentToolProvider, LocalAgentToolProvider>();
builder.Services.AddScoped<IRepositoryRetriever, LocalRepositoryRetriever>();
builder.Services.AddScoped<RepositoryRagContextBuilder>();
builder.Services.AddScoped<SpecializedReviewOrchestrator>();
builder.Services.AddScoped<ISpecializedReviewAgent, SecurityReviewAgent>();
builder.Services.AddScoped<ISpecializedReviewAgent, TestingReviewAgent>();
builder.Services.AddScoped<AiBudgetOptions>();
builder.Services.AddScoped<IAiBudgetGuard, AiBudgetGuard>();
builder.Services.AddScoped<AiUsageMetrics>();
builder.Services.AddScoped<IAgentPipeline, AgentPipeline>();
builder.Services.AddScoped<IAgentPipelineStage, OrchestrationStage>();
builder.Services.AddScoped<IAgentPipelineStage, RagContextStage>();
builder.Services.AddScoped<IAgentPipelineStage, SpecializedReviewStage>();
builder.Services.AddScoped<IAgentPipelineStage, PromptBuildStage>();
builder.Services.AddScoped<IAgentPipelineStage, LlmReviewStage>();
builder.Services.AddScoped<PipelineExecutionMetrics>();




using var host = builder.Build();

if (args.Length >= 1 && args[0] == "analyze-pr")
{
    var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
    var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

    if (string.IsNullOrWhiteSpace(repository))
        throw new InvalidOperationException("No se encontró GITHUB_REPOSITORY.");

    if (string.IsNullOrWhiteSpace(githubToken))
        throw new InvalidOperationException("No se encontró GITHUB_TOKEN.");

    var workspacePath = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")
        ?? Directory.GetCurrentDirectory();

    using var prScope = host.Services.CreateScope();

    var workflow = prScope.ServiceProvider
        .GetRequiredService<IPullRequestReviewWorkflow>();

    await workflow.ExecuteAsync(
        new PullRequestReviewWorkflowRequest
        {
            Repository = repository,
            GitHubToken = githubToken,
            PullRequestNumber = GitHubEventReader.GetPullRequestNumber(),
            WorkspacePath = workspacePath
        },
        CancellationToken.None);

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
