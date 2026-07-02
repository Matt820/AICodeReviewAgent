namespace AiCodeReviewAgent.Application.Reviews.Rules;

public interface ICodeReviewRule
{
    IEnumerable<CodeReviewFinding> Analyze(string code);
}