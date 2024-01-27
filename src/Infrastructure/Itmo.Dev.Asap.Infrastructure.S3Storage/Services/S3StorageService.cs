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
    private readonly S3StorageOptions _options;

    public S3StorageService(
        IOptionsMonitor<S3StorageOptions> optionsMonitor,
        IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
        _options = optionsMonitor.CurrentValue;

        var config = new AmazonS3Config { ServiceURL = _options.ServiceUrl.ToString() };
        _client = new AmazonS3Client(_options.KeyId, _options.SecretAccessKey, config);
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
            Expires = _dateTimeProvider.Current.Add(_options.LinkTimeToLive).DateTime,
        };

        string link = _client.GetPreSignedURL(preSignRequest);

        return new StoredData(link);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}