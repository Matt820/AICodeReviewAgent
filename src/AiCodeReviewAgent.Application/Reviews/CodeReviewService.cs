using System;
using System.Collections.Generic;
using System.Text;
using AiCodeReviewAgent.Application.Reviews.Rules;

namespace AiCodeReviewAgent.Application.Reviews;

public sealed class CodeReviewService : ICodeReviewService
{
    private readonly IEnumerable<ICodeReviewRule> _rules;

    public CodeReviewService(IEnumerable<ICodeReviewRule> rules)
    {
        _rules = rules;
    }

    public Task<AnalyzeCodeResponse> AnalyzeAsync(
        AnalyzeCodeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return Task.FromResult(new AnalyzeCodeResponse
            {
                Summary = "El código enviado está vacío.",
                Findings =
                [
                    new CodeReviewFinding
                    {
                        Severity = "High",
                        Message = "El código enviado está vacío.",
                        Line = null
                    }
                ]
            });
        }

        var findings = _rules
            .SelectMany(rule => rule.Analyze(request.Code))
            .ToList();

        return Task.FromResult(new AnalyzeCodeResponse
        {
            Summary = findings.Count == 0
                ? "No se encontraron observaciones básicas."
                : $"Se encontraron {findings.Count} observaciones.",
            Findings = findings
        });
    }
}
