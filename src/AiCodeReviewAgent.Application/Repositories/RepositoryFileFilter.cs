namespace AiCodeReviewAgent.Application.Repositories;

public static class RepositoryFileFilter
{
    public static bool ShouldAnalyze(string filePath)
    {
        var normalized = filePath.Replace("\\", "/");

        if (normalized.Contains("/bin/")) return false;
        if (normalized.Contains("/obj/")) return false;
        if (normalized.Contains("/tests/")) return false;
        if (normalized.Contains("/Rules/")) return false;
        if (normalized.EndsWith("/Class1.cs")) return false;

        return normalized.EndsWith(".cs");
    }
}