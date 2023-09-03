using Itmo.Dev.Asap.Github.Presentation.Grpc.Controllers;
using Microsoft.AspNetCore.Builder;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGrpcPresentation(this IApplicationBuilder builder)
    {
        builder.UseEndpoints(x =>
        {
            x.MapGrpcService<GithubManagementController>();
            x.MapGrpcService<GithubSubjectCourseController>();
            x.MapGrpcService<GithubUserController>();
            x.MapGrpcService<GithubSearchController>();
            x.MapGrpcReflectionService();
        });

        return builder;
    }
}