#pragma warning disable CA1506

using Itmo.Dev.Asap.Github.Application.Extensions;
using Itmo.Dev.Asap.Github.Caching.Extensions;
using Itmo.Dev.Asap.Github.Common.Extensions;
using Itmo.Dev.Asap.Github.DataAccess.Extensions;
using Itmo.Dev.Asap.Github.Integrations.Core.Extensions;
using Itmo.Dev.Asap.Github.Octokit.Extensions;
using Itmo.Dev.Asap.Github.Presentation.Grpc.Extensions;
using Itmo.Dev.Asap.Github.Presentation.Kafka.Extensions;
using Itmo.Dev.Asap.Github.Presentation.Webhooks.Extensions;
using Itmo.Dev.Asap.Infrastructure.Locking.Extensions;
using Itmo.Dev.Asap.Infrastructure.S3Storage.Extensions;
using Itmo.Dev.Platform.BackgroundTasks.Extensions;
using Itmo.Dev.Platform.Logging.Extensions;
using Itmo.Dev.Platform.YandexCloud.Extensions;
using Prometheus;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

await builder.AddYandexCloudConfigurationAsync();

builder.AddPlatformSentry();
builder.Host.AddPlatformSerilog(builder.Configuration);

builder.Services
    .AddCommon()
    .AddApplication()
    .AddGithubCaching(builder.Configuration)
    .AddDataAccess()
    .AddOctokitIntegration(builder.Configuration)
    .AddInfrastructureLocking()
    .AddInfrastructureS3Storage()
    .AddCoreIntegration()
    .AddGrpcPresentation()
    .AddWebhooksPresentation()
    .AddKafkaPresentation(builder.Configuration);

builder.Services.AddPlatformBackgroundTasks(configurator => configurator
    .ConfigurePersistence(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Persistence"))
    .ConfigureScheduling(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Scheduling"))
    .ConfigureExecution(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Execution"))
    .AddApplicationBackgroundTasks());

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

WebApplication app = builder.Build();

await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
{
    await scope.UseDataAccessAsync(default);
    await scope.UsePlatformBackgroundTasksAsync(default);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(o => o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseRouting();
app.UseHttpMetrics();
app.UseGrpcMetrics();
app.UsePlatformSentryTracing(builder.Configuration);

app.UseGrpcPresentation();
app.UseWebhooksPresentation();

app.MapMetrics();

await app.RunAsync();