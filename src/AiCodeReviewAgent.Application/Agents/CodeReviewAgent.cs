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
    private readonly IAgentPipeline _pipeline;

    public CodeReviewAgent(        
        IAgentPipeline pipeline)
    {        
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

        var pipelineContext = new AgentPipelineContext
        {
            RepositoryPath = repositoryPath,
            ChangedFilePath = changedFilePath,
            Patch = patch,
            AgentContext = context
        };

        await _pipeline.ExecuteAsync(
            pipelineContext,
            cancellationToken);

        return pipelineContext.ReviewMarkdown;         
    }
}