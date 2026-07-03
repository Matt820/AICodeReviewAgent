using System.Text.Json;

public static class GitHubEventReader
{
    public static int GetPullRequestNumber()
    {
        var eventPath = Environment.GetEnvironmentVariable("GITHUB_EVENT_PATH");

        if (string.IsNullOrWhiteSpace(eventPath))
            throw new InvalidOperationException("No se encontró GITHUB_EVENT_PATH.");

        var json = File.ReadAllText(eventPath);

        using var document = JsonDocument.Parse(json);

        return document.RootElement
            .GetProperty("pull_request")
            .GetProperty("number")
            .GetInt32();
    }
}