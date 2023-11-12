using Grpc.Net.ClientFactory;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubjectCourses;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.Submissions;
using Itmo.Dev.Asap.Github.Application.Abstractions.Integrations.Core.Services.SubmissionWorkflow;
using Itmo.Dev.Asap.Github.Integrations.Core.Services;
using Itmo.Dev.Asap.Github.Integrations.Core.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Itmo.Dev.Asap.Github.Integrations.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreIntegration(this IServiceCollection collection)
    {
        collection
            .AddOptions<CoreIntegrationOptions>()
            .BindConfiguration("Infrastructure:Integrations:Core");

        static void ConfigureAddress(IServiceProvider sp, GrpcClientFactoryOptions o)
        {
            IOptions<CoreIntegrationOptions> options = sp.GetRequiredService<IOptions<CoreIntegrationOptions>>();
            o.Address = options.Value.ServiceUri;
        }

        collection.AddGrpcClient<Asap.Core.Permissions.PermissionService.PermissionServiceClient>(ConfigureAddress);
        collection.AddScoped<IPermissionService, PermissionService>();

        collection
            .AddGrpcClient<Asap.Core.SubjectCourses.SubjectCourseService.SubjectCourseServiceClient>(ConfigureAddress);

        collection.AddScoped<ISubjectCourseService, SubjectCourseService>();

        collection.AddGrpcClient<Asap.Core.Submissions.SubmissionService.SubmissionServiceClient>(ConfigureAddress);
        collection.AddScoped<ISubmissionService, SubmissionService>();

        collection
            .AddGrpcClient<Asap.Core.SubmissionWorkflow.SubmissionWorkflowService.SubmissionWorkflowServiceClient>(
                ConfigureAddress);

        collection.AddScoped<ISubmissionWorkflowService, SubmissionWorkflowService>();

        collection.AddGrpcClient<Asap.Core.Users.UserService.UserServiceClient>(ConfigureAddress);
        collection.AddScoped<IUserService, UserService>();

        return collection;
    }
}