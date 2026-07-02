namespace AiCodeReviewAgent.Application.Reviews.Rules;

public sealed class AvoidConsoleWriteLineRule : ICodeReviewRule
{
    public IEnumerable<CodeReviewFinding> Analyze(string code)
    {
        var lines = code.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("Console.WriteLine"))
            {
                yield return new CodeReviewFinding
                {
                    Severity = "Low",
                    Message = "Evita dejar Console.WriteLine en código de producción. Usa ILogger.",
                    Line = i + 1
                };
            }
        }
    }
}