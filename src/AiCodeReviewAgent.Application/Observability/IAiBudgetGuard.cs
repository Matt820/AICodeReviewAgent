namespace AiCodeReviewAgent.Application.Observability;

public interface IAiBudgetGuard
{
    bool CanExecuteAiCall(string input);

    string CreateBudgetExceededMessage();
}