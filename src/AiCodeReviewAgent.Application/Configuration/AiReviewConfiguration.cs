namespace AiCodeReviewAgent.Application.Configuration;

public sealed class AiReviewConfiguration
{
    public string Language { get; set; } = "csharp";
    public int MaxFiles { get; set; } = 10;
    public List<string> Rules { get; set; } = [];
    public AiReviewFeatureOptions Features { get; set; } = new();
}