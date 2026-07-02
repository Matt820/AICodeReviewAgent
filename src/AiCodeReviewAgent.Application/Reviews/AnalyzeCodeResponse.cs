using System;
using System.Collections.Generic;
using System.Text;

namespace AiCodeReviewAgent.Application.Reviews
{
    public sealed class AnalyzeCodeResponse
    {
        public string Summary { get; set; } = string.Empty;
        public List<CodeReviewFinding> Findings { get; set; } = [];
    }
}
