namespace AiCodeReviewAgent.Application.Observability;

public interface IAiBudgetGuard
{
    bool WasBudgetExceeded { get; }
    bool CanExecuteAiCall(string input);

    string CreateBudgetExceededMessage();
}