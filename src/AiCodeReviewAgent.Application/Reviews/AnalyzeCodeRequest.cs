using System;
using System.Collections.Generic;
using System.Text;

namespace AiCodeReviewAgent.Application.Reviews
{
    public sealed class AnalyzeCodeRequest
    {
        public string FileName { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
