using System;
using System.Collections.Generic;
using System.Text;

namespace AiCodeReviewAgent.Application.Reviews
{
    public sealed class CodeReviewFinding
    {
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? Line { get; set; }
    }
}
