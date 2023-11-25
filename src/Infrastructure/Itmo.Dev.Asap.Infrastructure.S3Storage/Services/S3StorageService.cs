using Amazon.S3;
using Amazon.S3.Model;
using Itmo.Dev.Asap.Github.Application.Abstractions.Storage;
using Itmo.Dev.Asap.Infrastructure.S3Storage.Options;
using Itmo.Dev.Platform.Common.DateTime;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Infrastructure.S3Storage.Services;

public class S3StorageService : IStorageService, IDisposable
{
    private readonly AmazonS3Client _client;
    private readonly IDateTimeProvider _dateTimeProvider;

    public S3StorageService(IOptionsMonitor<S3StorageOptions> optionsMonitor, IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        S3StorageOptions options = optionsMonitor.CurrentValue;

        var config = new AmazonS3Config { ServiceURL = options.ServiceUrl.ToString() };
        _client = new AmazonS3Client(options.KeyId, options.SecretAccessKey, config);
    }

    public async Task<StoredData> StoreAsync(string bucketName, Stream content, CancellationToken cancellationToken)
    {
        string key = Guid.NewGuid().ToString();

        var request = new PutObjectRequest
        {
            Key = key,
            BucketName = bucketName,
            InputStream = content,
            AutoCloseStream = false,
        };

        await _client.PutObjectAsync(request, cancellationToken);

        var preSignRequest = new GetPreSignedUrlRequest
        {
            Key = key,
            BucketName = bucketName,
            Expires = _dateTimeProvider.Current.AddHours(48).DateTime,
        };

        string link = _client.GetPreSignedURL(preSignRequest);

        return new StoredData(link);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}