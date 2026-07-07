using System.Text.Json;
using AiCodeReviewAgent.Application.Agents.Orchestration;
using AiCodeReviewAgent.Application.Reviews;

namespace AiCodeReviewAgent.Application.Agents.Planning;

public sealed class LlmAgentPlanner : IAgentPlanner
{
    private readonly IAiCodeReviewClient _aiClient;
    private readonly HeuristicAgentPlanner _fallbackPlanner;

    public LlmAgentPlanner(
        IAiCodeReviewClient aiClient,
        HeuristicAgentPlanner fallbackPlanner)
    {
        _aiClient = aiClient;
        _fallbackPlanner = fallbackPlanner;
    }

    public async Task<AgentPlan> CreatePlanAsync(
        AgentContext context,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPlannerPrompt(context);

        try
        {
            var response = await _aiClient.AnalyzeCodeAsync(
                new AnalyzeCodeRequest
                {
                    FileName = "agent-plan",
                    Language = "json",
                    Code = prompt
                },
                cancellationToken);

            var plan = ParsePlan(response);

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

    private static string BuildPlannerPrompt(AgentContext context)
    {
        return $$"""
        Eres un planner de herramientas para un AI Code Review Agent.

        Tu tarea es decidir qué herramientas ejecutar antes de hacer el review del Pull Request.

        Herramientas disponibles:

        1. read_file
        - Lee el contenido completo de un archivo.
        - Input esperado: ruta relativa del archivo dentro del repositorio.

        2. search_text
        - Busca referencias de un texto dentro del repositorio.
        - Input esperado: texto a buscar, por ejemplo nombre de clase, interfaz o método.

        3. read_solution
        - Lee la solución .sln para entender estructura de proyectos.
        - Input esperado: vacío.

        4. read_project_file
        - Lee archivos .csproj para entender dependencias.
        - Input esperado: vacío.

        5. find_class
        - Busca la definición de una clase.
        - Input esperado: nombre exacto de la clase.

        6. find_interface
        - Busca la definición de una interfaz.
        - Input esperado: nombre exacto de la interfaz.

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

    private static AgentPlan ParsePlan(string response)
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

        /* var allowedTools = new HashSet<string>(
            ["read_file", "search_text"],
            StringComparer.OrdinalIgnoreCase); */
        
        var allowedTools = new HashSet<string>(
            [
                "read_file",
                "search_text",
                "read_solution",
                "read_project_file",
                "find_class",
                "find_interface"
            ],
            StringComparer.OrdinalIgnoreCase);

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