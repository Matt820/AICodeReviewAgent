using AiCodeReviewAgent.Application.Agents.Orchestration;

namespace AiCodeReviewAgent.Application.Agents.Planning;

public sealed class HeuristicAgentPlanner : IAgentPlanner
{
    public Task<AgentPlan> CreatePlanAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var steps = new List<AgentStep>();

        var changedFiles = ExtractChangedFiles(context.PullRequestDiff);

        foreach (var file in changedFiles.Take(5))
        {
            if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                steps.Add(new AgentStep
                {
                    ToolName = "read_file",
                    Input = file,
                    Required = false
                });

                if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    steps.Add(new AgentStep
                    {
                        ToolName = "search_text",
                        Input = Path.GetFileNameWithoutExtension(file),
                        Required = false
                    });
                }
            }
        }

        return Task.FromResult(new AgentPlan
        {
            Steps = steps
        });
    }

    private static List<string> ExtractChangedFiles(string diff)
    {
        return diff
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("diff --git ", StringComparison.Ordinal))
            .Select(line =>
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var bPath = parts.Length >= 4
                    ? parts[3]
                    : string.Empty;

                return bPath.StartsWith("b/")
                    ? bPath[2..]
                    : bPath;
            })
            .Where(file => !string.IsNullOrWhiteSpace(file))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}