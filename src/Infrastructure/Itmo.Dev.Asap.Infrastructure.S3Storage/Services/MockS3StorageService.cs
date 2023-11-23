using Itmo.Dev.Asap.Github.Application.Abstractions.Storage;

namespace Itmo.Dev.Asap.Infrastructure.S3Storage.Services;

public class MockS3StorageService : IStorageService
{
    public Task<StoredData> StoreAsync(string bucketName, Stream content, CancellationToken cancellationToken)
    {
        string key = Guid.NewGuid().ToString();
        return Task.FromResult(new StoredData(key));
    }
}