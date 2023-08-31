using Newtonsoft.Json;

namespace Itmo.Dev.Asap.Github.Octokit.Extensions;

internal static class JsonSerializerExtensions
{
    public static async Task<T?> DeserializeAsync<T>(
        this HttpContent content,
        JsonSerializer serializer,
        CancellationToken cancellationToken)
    {
        await using Stream stream = await content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        await using var jsonReader = new JsonTextReader(reader);

        return serializer.Deserialize<T>(jsonReader);
    }
}