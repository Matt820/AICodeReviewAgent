using AiCodeReviewAgent.Application.Reviews;
using AiCodeReviewAgent.Application.Agents.Orchestration;
using AiCodeReviewAgent.Application.Agents.Prompts;
using AiCodeReviewAgent.Application.Rag;
using AiCodeReviewAgent.Application.Agents.Specialized;
using AiCodeReviewAgent.Application.Configuration;
using AiCodeReviewAgent.Application.Agents.Pipeline;

namespace AiCodeReviewAgent.Application.Agents;

public interface ICodeReviewAgent
{
    Task<string> ReviewPullRequestAsync(
        string repositoryPath,
        string changedFilePath,
        string patch,
        AgentToolResult? buildResult,
        AgentToolResult? testResult,
        List<string> rules,
        AiReviewFeatureOptions features,
        CancellationToken cancellationToken);
}

public sealed class CodeReviewAgent : ICodeReviewAgent
{
    //private readonly IEnumerable<IAgentTool> _tools;
    private readonly IAiCodeReviewClient _aiClient;
    private readonly IAgentOrchestrator _orchestrator;
    private readonly ICodeReviewPromptBuilder _promptBuilder;
    private readonly RepositoryRagContextBuilder _ragContextBuilder;
    private readonly SpecializedReviewOrchestrator _specializedReviewOrchestrator;
    private readonly IAgentPipeline _pipeline;

    public CodeReviewAgent(
        //IEnumerable<IAgentTool> tools,
        IAgentOrchestrator orchestrator,
        ICodeReviewPromptBuilder promptBuilder,
        RepositoryRagContextBuilder reagContextBuilder,
        SpecializedReviewOrchestrator specializedReviewOrchestrator,
        IAiCodeReviewClient aiClient,
        IAgentPipeline pipeline)
    {
        //_tools = tools;
        _orchestrator = orchestrator;
        _promptBuilder = promptBuilder;
        _ragContextBuilder = reagContextBuilder;
        _specializedReviewOrchestrator = specializedReviewOrchestrator;
        _aiClient = aiClient;
        _pipeline = pipeline;
    }

    public async Task<string> ReviewPullRequestAsync(
        string repositoryPath,
        string changedFilePath,
        string patch,
        AgentToolResult? buildResult,
        AgentToolResult? testResult,
        List<string> rules,
        AiReviewFeatureOptions features,
        CancellationToken cancellationToken)
    {
        var context = new AgentContext
        {
            RepositoryPath = repositoryPath,
            PullRequestDiff = patch,
            BuildResult = buildResult,
            TestResult = testResult,
            Rules = rules,
            UseLlmPlanner = features.LlmPlanner,
            Features = features
        };

        /* var result = await _pipeline.ExecuteAsync(
            new AgentPipelineContext
            {
                RepositoryPath = repositoryPath,
                ChangedFilePath = changedFilePath,
                Patch = patch,
                AgentContext = agentContext
            },
            cancellationToken);

        return result.ReviewMarkdown; */

        await _orchestrator.ExecuteAsync(
            context,
            cancellationToken);

        if (features.Rag)
        {
            context.RagContext = await _ragContextBuilder.BuildAsync(
                repositoryPath,
                $"{changedFilePath} {Path.GetFileNameWithoutExtension(changedFilePath)}",
                cancellationToken);
        }

        if(features.SpecializedAgents)
        {
            var specializedReviews = await _specializedReviewOrchestrator.ReviewAsync(
                new SpecializedReviewAgentRequest
                {
                    ChangedFilePath = changedFilePath,
                    Context = context
                },
                cancellationToken);

            context.SpecializedReviews.AddRange(specializedReviews);
        }             
        
        var prompt = _promptBuilder.Build(
            new CodeReviewPromptContext
            {
                ChangedFilePath = changedFilePath,
                AgentContext = context
            });   

        return await _aiClient.AnalyzeCodeAsync(
            new AnalyzeCodeRequest
            {
                FileName = changedFilePath,
                Language = "diff",
                Code = prompt
            },
            cancellationToken);              
    }
}