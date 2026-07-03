using AiCodeReviewAgent.Application.Reviews;

namespace AiCodeReviewAgent.Application.Repositories;

public interface IAiRepositoryAnalysisService
{
    Task<AiRepositoryAnalysisResponse> AnalyzeLocalWithAiAsync(
        AnalyzeLocalRepositoryRequest request,
        CancellationToken cancellationToken);
}

public sealed class AiRepositoryAnalysisService : IAiRepositoryAnalysisService
{
    private readonly IAiCodeReviewClient _aiCodeReviewClient;

    public AiRepositoryAnalysisService(IAiCodeReviewClient aiCodeReviewClient)
    {
        _aiCodeReviewClient = aiCodeReviewClient;
    }

    public async Task<AiRepositoryAnalysisResponse> AnalyzeLocalWithAiAsync(
        AnalyzeLocalRepositoryRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RepositoryPath))
            throw new ArgumentException("La ruta del repositorio es requerida.");

        if (!Directory.Exists(request.RepositoryPath))
            throw new DirectoryNotFoundException($"No existe la ruta: {request.RepositoryPath}");

        var maxFiles = request.MaxFiles <= 0 ? 5 : request.MaxFiles;

        var files = Directory
            .GetFiles(request.RepositoryPath, "*.cs", SearchOption.AllDirectories)
            .Where(RepositoryFileFilter.ShouldAnalyze)
            .Take(maxFiles)
            .ToList();

        var response = new AiRepositoryAnalysisResponse
        {
            RepositoryPath = request.RepositoryPath,
            FilesAnalyzed = files.Count
        };

        foreach (var file in files)
        {
            var code = await File.ReadAllTextAsync(file, cancellationToken);

            var aiReview = await _aiCodeReviewClient.AnalyzeCodeAsync(
                new AnalyzeCodeRequest
                {
                    FileName = Path.GetFileName(file),
                    Language = "csharp",
                    Code = code
                },
                cancellationToken);

            response.Files.Add(new AiFileAnalysisResult
            {
                FilePath = file,
                AiReview = aiReview
            });
        }

        return response;
    }
}