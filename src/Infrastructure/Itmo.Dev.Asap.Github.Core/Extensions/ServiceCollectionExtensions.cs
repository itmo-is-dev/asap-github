using Itmo.Dev.Asap.Github.Application.Core.Services;
using Itmo.Dev.Asap.Github.Core.Dummy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Github.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreIntegration(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        if (configuration.GetSection("Infrastructure:Integrations:Core:Enabled").Get<bool>())
        {
            throw new NotImplementedException("Real core integration is not implemented");
        }
        else
        {
            collection.AddSingleton<IPermissionService, DummyPermissionService>();
            collection.AddSingleton<ISubjectCourseService, DummySubjectCourseService>();
            collection.AddSingleton<ISubmissionService, DummySubmissionService>();
            collection.AddSingleton<ISubmissionWorkflowService, DummySubmissionWorkflowService>();
            collection.AddSingleton<IUserService, DummyUserService>();
        }

        return collection;
    }
}