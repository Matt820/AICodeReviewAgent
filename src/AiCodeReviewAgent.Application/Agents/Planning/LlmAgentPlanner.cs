using System.Text.Json;
using AiCodeReviewAgent.Application.Agents.Orchestration;
using AiCodeReviewAgent.Application.Agents.Tools;
using AiCodeReviewAgent.Application.Reviews;

namespace AiCodeReviewAgent.Application.Agents.Planning;

public sealed class LlmAgentPlanner : IAgentPlanner
{
    private readonly IAiCodeReviewClient _aiClient;
    private readonly HeuristicAgentPlanner _fallbackPlanner;
    private readonly IAgentToolProvider _toolProvider;

    public LlmAgentPlanner(
        IAiCodeReviewClient aiClient,
        HeuristicAgentPlanner fallbackPlanner,
        IAgentToolProvider toolProvider)
    {
        _aiClient = aiClient;
        _fallbackPlanner = fallbackPlanner;
        _toolProvider = toolProvider;
    }

    public async Task<AgentPlan> CreatePlanAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var availableTools = await _toolProvider.ListToolsAsync(cancellationToken);
        var prompt = BuildPlannerPrompt(context, availableTools);

        try
        {

            if (!context.UseLlmPlanner)
            {
                return await _fallbackPlanner.CreatePlanAsync(
                    context,
                    cancellationToken);
            }

            var response = await _aiClient.AnalyzeCodeAsync(
                new AnalyzeCodeRequest
                {
                    FileName = "agent-plan",
                    Language = "json",
                    Code = prompt
                },
                cancellationToken);

            var plan = ParsePlan(response, availableTools);

            if (plan.Steps.Count == 0)
            {
                return await _fallbackPlanner.CreatePlanAsync(
                    context,
                    cancellationToken);
            }

            return plan;
        }
        catch
        {
            return await _fallbackPlanner.CreatePlanAsync(
                context,
                cancellationToken);
        }
    }

    private static string BuildPlannerPrompt(AgentContext context, IReadOnlyList<AgentToolDefinition> availableTools)
    {
        var toolsDescription = string.Join(
            Environment.NewLine,
            availableTools.Select(tool =>
                $"""
                Tool: {tool.Name}
                Description: {tool.Description}
                Input: string
                """));
        return $$"""
        Eres un planner de herramientas para un AI Code Review Agent.

        Tu tarea es decidir qué herramientas ejecutar antes de hacer el review del Pull Request.

        Herramientas disponibles:
        {{toolsDescription}}        

        Reglas:
        - Devuelve SOLO JSON válido.
        - No incluyas Markdown.
        - No incluyas explicaciones.
        - Máximo 4 steps.
        - Usa read_file para archivos importantes modificados.
        - Usa search_text si necesitas buscar referencias de clases, métodos o interfaces.
        - No inventes rutas que no aparezcan en el diff.
        - Si no estás seguro, usa pocos steps.
        - Los tools opcionales deben tener required = false.

        Formato exacto:

        {
        "steps": [
            {
            "toolName": "read_file",
            "input": "src/Example/File.cs",
            "required": false
            }
        ]
        }

        Diff del Pull Request:
        ```diff
        {{AgentTextLimiter.Limit(context.PullRequestDiff, 8000)}}
        ```
        """;
    }

    private static AgentPlan ParsePlan(string response, IReadOnlyList<AgentToolDefinition> availableTools)
    {
        var json = ExtractJson(response);

        var parsed = JsonSerializer.Deserialize<LlmAgentPlanResponse>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (parsed is null)
        {
            return new AgentPlan();
        }
        
        var allowedTools = availableTools
            .Select(x => x.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var steps = parsed.Steps
            .Where(x => allowedTools.Contains(x.ToolName))
            .Where(x => !string.IsNullOrWhiteSpace(x.Input))
            .Take(4)
            .Select(x => new AgentStep
            {
                ToolName = x.ToolName,
                Input = x.Input,
                Required = x.Required
            })
            .ToList();

        return new AgentPlan
        {
            Steps = steps
        };
    }

    private static string ExtractJson(string response)
    {
        var start = response.IndexOf('{');
        var end = response.LastIndexOf('}');

        if (start < 0 || end < 0 || end <= start)
        {
            throw new InvalidOperationException("No se encontró JSON válido en la respuesta del planner.");
        }

        return response[start..(end + 1)];
    }
}