using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AiCodeReviewAgent.Application.Configuration;

public interface IAiReviewConfigurationLoader
{
    Task<AiReviewConfiguration> LoadAsync(
        string repositoryPath,
        CancellationToken cancellationToken);
}

public sealed class AiReviewConfigurationLoader : IAiReviewConfigurationLoader
{
    public async Task<AiReviewConfiguration> LoadAsync(
        string repositoryPath,
        CancellationToken cancellationToken)
    {
        var configPath = Path.Combine(repositoryPath, ".ai-review.yml");

        if (!File.Exists(configPath))
            return new AiReviewConfiguration();

        var yaml = await File.ReadAllTextAsync(configPath, cancellationToken);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<AiReviewConfiguration>(yaml)
               ?? new AiReviewConfiguration();
    }
}