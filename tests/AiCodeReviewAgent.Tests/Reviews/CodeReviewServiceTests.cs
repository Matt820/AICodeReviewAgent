using AiCodeReviewAgent.Application.Reviews;
using AiCodeReviewAgent.Application.Reviews.Rules;
using FluentAssertions;

namespace AiCodeReviewAgent.Tests.Reviews;

public class CodeReviewServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_ShouldDetectConsoleWriteLine()
    {
        var rules = new ICodeReviewRule[]
        {
            new AvoidConsoleWriteLineRule(),
            new AvoidGenericExceptionCatchRule()
        };

        var service = new CodeReviewService(rules);

        var request = new AnalyzeCodeRequest
        {
            FileName = "Test.cs",
            Language = "csharp",
            Code = "public void Run()\n{\n Console.WriteLine(\"Hola\");\n}"
        };

        var result = await service.AnalyzeAsync(request, CancellationToken.None);

        result.Findings.Should().HaveCount(1);
        result.Findings[0].Severity.Should().Be("Low");
        result.Findings[0].Line.Should().Be(3);
    }

    [Fact]
    public async Task AnalyzeAsync_ShouldDetectGenericExceptionCatch()
    {
        var rules = new ICodeReviewRule[]
        {
            new AvoidConsoleWriteLineRule(),
            new AvoidGenericExceptionCatchRule()
        };

        var service = new CodeReviewService(rules);

        var request = new AnalyzeCodeRequest
        {
            FileName = "Service.cs",
            Language = "csharp",
            Code = "try\n{\n}\ncatch (Exception ex)\n{\n}"
        };

        var result = await service.AnalyzeAsync(request, CancellationToken.None);

        result.Findings.Should().HaveCount(1);
        result.Findings[0].Severity.Should().Be("Medium");
        result.Findings[0].Line.Should().Be(4);
    }
}