namespace AiCodeReviewAgent.Application.Reviews.Rules;

public sealed class AvoidGenericExceptionCatchRule : ICodeReviewRule
{
    public IEnumerable<CodeReviewFinding> Analyze(string code)
    {
        var lines = code.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("catch (Exception"))
            {
                yield return new CodeReviewFinding
                {
                    Severity = "Medium",
                    Message = "Evita capturar Exception de forma genérica sin manejo específico.",
                    Line = i + 1
                };
            }
        }
    }
}