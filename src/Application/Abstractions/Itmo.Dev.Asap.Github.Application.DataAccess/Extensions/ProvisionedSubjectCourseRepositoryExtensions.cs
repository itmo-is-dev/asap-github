using Itmo.Dev.Asap.Github.Application.DataAccess.Repositories;

namespace Itmo.Dev.Asap.Github.Application.DataAccess.Extensions;

public static class ProvisionedSubjectCourseRepositoryExtensions
{
    public static void Remove(this IProvisionedSubjectCourseRepository repository, string correlationId)
    {
        repository.RemoveRange(new[] { correlationId });
    }
}