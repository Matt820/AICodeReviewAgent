namespace AiCodeReviewAgent.Application.Agents.Prompts;

public interface ICodeReviewPromptBuilder
{
    string Build(CodeReviewPromptContext context);
}