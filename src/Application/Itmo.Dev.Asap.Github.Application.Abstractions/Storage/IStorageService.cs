namespace Itmo.Dev.Asap.Github.Application.Abstractions.Storage;

public interface IStorageService
{
    Task<StoredData> StoreAsync(string bucketName, Stream content, CancellationToken cancellationToken);
}