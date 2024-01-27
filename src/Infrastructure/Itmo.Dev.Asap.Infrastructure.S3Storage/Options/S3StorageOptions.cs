using System.ComponentModel.DataAnnotations;

namespace Itmo.Dev.Asap.Infrastructure.S3Storage.Options;

public class S3StorageOptions : IValidatableObject
{
    public bool IsEnabled { get; set; }

    public string KeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;

    public Uri ServiceUrl { get; set; } = null!;

    public TimeSpan LinkTimeToLive { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsEnabled is false)
            yield break;

        if (string.IsNullOrEmpty(KeyId))
            yield return new ValidationResult("KeyId must be defined");

        if (string.IsNullOrEmpty(SecretAccessKey))
            yield return new ValidationResult("SecretAccessKey must be defined");

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (ServiceUrl is null)
            yield return new ValidationResult("ServiceUrl must be defined");
    }
}