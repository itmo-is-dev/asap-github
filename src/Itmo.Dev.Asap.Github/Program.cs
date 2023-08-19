#pragma warning disable CA1506

using Itmo.Dev.Asap.Github.Application.Extensions;
using Itmo.Dev.Asap.Github.Application.Handlers.Extensions;
using Itmo.Dev.Asap.Github.Caching.Extensions;
using Itmo.Dev.Asap.Github.DataAccess.Extensions;
using Itmo.Dev.Asap.Github.Integrations.Core.Extensions;
using Itmo.Dev.Asap.Github.Octokit.Extensions;
using Itmo.Dev.Asap.Github.Presentation.Grpc.Extensions;
using Itmo.Dev.Asap.Github.Presentation.Kafka.Extensions;
using Itmo.Dev.Asap.Github.Presentation.Webhooks.Extensions;
using Itmo.Dev.Platform.Logging.Extensions;
using Itmo.Dev.Platform.YandexCloud.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

await builder.AddYandexCloudConfigurationAsync();

builder.AddPlatformSentry();
builder.Host.AddPlatformSerilog(builder.Configuration);

builder.Services
    .AddApplication()
    .AddApplicationHandlers()
    .AddGithubCaching()
    .AddDataAccess()
    .AddOctokitIntegration(builder.Configuration)
    .AddCoreIntegration()
    .AddGrpcPresentation()
    .AddWebhooksPresentation()
    .AddKafkaPresentation(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

WebApplication app = builder.Build();

await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    await scope.UseDataAccessAsync(default);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(o => o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseRouting();
app.UsePlatformSentryTracing(builder.Configuration);

app.UseGrpcPresentation();
app.UseWebhooksPresentation();

await app.RunAsync();