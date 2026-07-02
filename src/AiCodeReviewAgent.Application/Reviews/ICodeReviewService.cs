using System;
using System.Collections.Generic;
using System.Text;

namespace AiCodeReviewAgent.Application.Reviews
{
    public interface ICodeReviewService
    {
        Task<AnalyzeCodeResponse> AnalyzeAsync(AnalyzeCodeRequest request, CancellationToken cancellationToken);
    }
}
