using System.Diagnostics;
using AiCodeReviewAgent.Application.Agents;

namespace AiCodeReviewAgent.Application.Tools;

public sealed class RunTestsTool : IAgentTool
{
    public string Name => "run_tests";

    public string Description => "Ejecuta dotnet test en el repositorio.";

    public async Task<AgentToolResult> ExecuteAsync(
        string repositoryPath,
        string input,
        CancellationToken cancellationToken)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "test --configuration Release --no-build",
                    WorkingDirectory = repositoryPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            return new AgentToolResult
            {
                ToolName = Name,
                Success = process.ExitCode == 0,
                Output = string.IsNullOrWhiteSpace(output) ? error : output,
                Error = process.ExitCode == 0 ? null : error
            };
        }
        catch (Exception ex)
        {
            return new AgentToolResult
            {
                ToolName = Name,
                Success = false,
                Error = ex.Message
            };
        }
    }
}