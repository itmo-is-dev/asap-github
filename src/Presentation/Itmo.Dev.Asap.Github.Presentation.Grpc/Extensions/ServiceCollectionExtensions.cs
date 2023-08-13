using Microsoft.Extensions.DependencyInjection;

namespace Itmo.Dev.Asap.Github.Presentation.Grpc.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcPresentation(this IServiceCollection collection)
    {
        collection.AddGrpc();
        collection.AddGrpcReflection();

        return collection;
    }
}