namespace AiCodeReviewAgent.Application.Agents;

public static class ReviewScoreCalculator
{
    public static int Calculate(AgentToolResult? buildResult, AgentToolResult? testResult, int filesReviewed)
    {
        var score = 100;

        if (buildResult?.Success != true)
            score -= 30;

        if (testResult?.Success != true)
            score -= 30;

        if (filesReviewed > 5)
            score -= 10;

        if (filesReviewed > 10)
            score -= 10;

        return Math.Clamp(score, 0, 100);
    }
}