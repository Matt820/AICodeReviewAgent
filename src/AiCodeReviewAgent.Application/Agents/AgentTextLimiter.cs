namespace AiCodeReviewAgent.Application.Agents;

public static class AgentTextLimiter
{
    public static string Limit(string? value, int maxCharacters = 4000)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Length <= maxCharacters
            ? value
            : value[..maxCharacters] + "\n\n[Contenido truncado por límite de contexto]";
    }
}