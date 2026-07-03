using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AiCodeReviewAgent.Application.Reviews;
using Microsoft.Extensions.Configuration;

namespace AiCodeReviewAgent.Infrastructure.Ai;

public sealed class OpenAiCodeReviewClient : IAiCodeReviewClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenAiCodeReviewClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> AnalyzeCodeAsync(
        AnalyzeCodeRequest request,
        CancellationToken cancellationToken)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API key no configurada.");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var prompt = $"""
        Actúa como un Senior Software Engineer especializado en .NET, Clean Architecture y seguridad.

        Analiza este archivo y devuelve un code review breve, claro y accionable.

        Archivo: {request.FileName}
        Lenguaje: {request.Language}

        Código:
        ```{request.Language}
        {request.Code}
        ```

        Responde en español con:
        - Resumen general
        - Problemas encontrados
        - Recomendaciones
        - Nivel de riesgo: Low, Medium o High
        """;

        var body = new
        {
            model = "gpt-4.1-mini",
            input = prompt,
            max_output_tokens = 700
        };

        var json = JsonSerializer.Serialize(body);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync(
            "https://api.openai.com/v1/responses",
            content,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine("OpenAI API Response:");

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(responseContent);

        using var document = JsonDocument.Parse(responseContent);

        /* return document.RootElement
            .GetProperty("output_text")
            .GetString() ?? string.Empty; */
        if (document.RootElement.TryGetProperty("output_text", out var outputText))
        {
            return outputText.GetString() ?? string.Empty;
        }

        if (document.RootElement.TryGetProperty("output", out var output))
        {
            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty("content", out var contentArray))
                    continue;

                foreach (var contentItem in contentArray.EnumerateArray())
                {
                    if (contentItem.TryGetProperty("text", out var text))
                        return text.GetString() ?? string.Empty;
                }
            }
        }

        return "No se pudo leer la respuesta del modelo.";
    }
}