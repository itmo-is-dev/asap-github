using Itmo.Dev.Asap.Github.Application.Handlers.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Github.Application.Handlers.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection collection)
    {
        collection
            .AddOptions<SubjectCourseOrganizationUpdateConfiguration>()
            .BindConfiguration("Application:SubjectCourseOrganizationUpdate");

        collection.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<IAssemblyMarker>());
        return collection;
    }
}