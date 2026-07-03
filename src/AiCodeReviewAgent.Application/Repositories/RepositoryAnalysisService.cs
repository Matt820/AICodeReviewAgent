using AiCodeReviewAgent.Application.Reviews;

namespace AiCodeReviewAgent.Application.Repositories;

public sealed class RepositoryAnalysisService : IRepositoryAnalysisService
{
    private readonly ICodeReviewService _codeReviewService;

    public RepositoryAnalysisService(ICodeReviewService codeReviewService)
    {
        _codeReviewService = codeReviewService;
    }

    public async Task<RepositoryAnalysisResponse> AnalyzeLocalAsync(
        AnalyzeLocalRepositoryRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RepositoryPath))
            throw new ArgumentException("La ruta del repositorio es requerida.");

        if (!Directory.Exists(request.RepositoryPath))
            throw new DirectoryNotFoundException($"No existe la ruta: {request.RepositoryPath}");

        /* var files = Directory
            .GetFiles(request.RepositoryPath, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
                !file.Contains("\\bin\\") &&
                !file.Contains("\\obj\\") &&
                !file.Contains("/bin/") &&
                !file.Contains("/obj/"))
            .ToList(); */
        var files = Directory
            .GetFiles(request.RepositoryPath, "*.cs", SearchOption.AllDirectories)
            .Where(RepositoryFileFilter.ShouldAnalyze)
            .ToList();

        var response = new RepositoryAnalysisResponse
        {
            RepositoryPath = request.RepositoryPath,
            FilesAnalyzed = files.Count
        };

        /* foreach (var file in files)
        {
            var code = await File.ReadAllTextAsync(file, cancellationToken);

            var analysis = await _codeReviewService.AnalyzeAsync(new AnalyzeCodeRequest
            {
                FileName = Path.GetFileName(file),
                Language = "csharp",
                Code = code
            }, cancellationToken);

            response.Files.Add(new FileAnalysisResult
            {
                FilePath = file,
                Analysis = analysis
            });
        }

        response.TotalFindings = response.Files.Sum(x => x.Analysis.Findings.Count); */
        foreach (var file in files)
        {
            var code = await File.ReadAllTextAsync(file, cancellationToken);

            var analysis = await _codeReviewService.AnalyzeAsync(new AnalyzeCodeRequest
            {
                FileName = Path.GetFileName(file),
                Language = "csharp",
                Code = code
            }, cancellationToken);

            response.Files.Add(new FileAnalysisResult
            {
                FilePath = file,
                Analysis = analysis
            });
        }

        var allFindings = response.Files
            .SelectMany(x => x.Analysis.Findings)
            .ToList();

        response.TotalFindings = allFindings.Count;
        response.HighFindings = allFindings.Count(x => x.Severity == "High");
        response.MediumFindings = allFindings.Count(x => x.Severity == "Medium");
        response.LowFindings = allFindings.Count(x => x.Severity == "Low");

        return response;
    }
}