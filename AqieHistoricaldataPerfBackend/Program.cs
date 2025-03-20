using AqieHistoricaldataPerfBackend.Example.Endpoints;
using AqieHistoricaldataPerfBackend.Example.Services;
using AqieHistoricaldataPerfBackend.Utils;
using AqieHistoricaldataPerfBackend.Utils.Http;
using AqieHistoricaldataPerfBackend.Utils.Mongo;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;
using AqieHistoricaldataPerfBackend.Config;
using AqieHistoricaldataPerfBackend.Utils.Logging;
using Serilog;
using AqieHistoricaldataPerfBackend.Atomfeed.Endpoints;
using AqieHistoricaldataPerfBackend.Atomfeed.Services;
using Microsoft.Net.Http.Headers;
using Amazon.S3;
using Hangfire;
using Hangfire.MemoryStorage;

var app = CreateWebApplication(args);
await app.RunAsync();
return;

[ExcludeFromCodeCoverage]
static WebApplication CreateWebApplication(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    ConfigureBuilder(builder);

    var app = builder.Build();
    return SetupApplication(app);
}

[ExcludeFromCodeCoverage]
static void ConfigureBuilder(WebApplicationBuilder builder)
{
    builder.Configuration.AddEnvironmentVariables();

    // Load certificates into Trust Store - Note must happen before Mongo and Http client connections.
    builder.Services.AddCustomTrustStore();
    
    // Configure logging to use the CDP Platform standards.
    builder.Services.AddHttpContextAccessor();
    builder.Host.UseSerilog(CdpLogging.Configuration);
    
    // Default HTTP Client
    //builder.Services
    //    .AddHttpClient("DefaultClient")
    //    .AddHeaderPropagation();

    // Proxy HTTP Client
    builder.Services.AddTransient<ProxyHttpMessageHandler>();
    builder.Services
        .AddHttpClient("proxy")
        .ConfigurePrimaryHttpMessageHandler<ProxyHttpMessageHandler>();

    builder.Services.AddHttpClient("Atomfeed", httpClient =>
    {
        httpClient.BaseAddress = new Uri("https://uk-air.defra.gov.uk/");
    
    }).ConfigurePrimaryHttpMessageHandler<ProxyHttpMessageHandler>(); ;
    
    

    // Propagate trace header.
    builder.Services.AddHeaderPropagation(options =>
    {
        var traceHeader = builder.Configuration.GetValue<string>("TraceHeader");
        if (!string.IsNullOrWhiteSpace(traceHeader))
        {
            options.Headers.Add(traceHeader);
        }
    });

    // Add Hangfire services.
    builder.Services.AddHangfire(config => config.UseMemoryStorage());
    builder.Services.AddHangfireServer();
    

    // Set up the MongoDB client. Config and credentials are injected automatically at runtime.
    builder.Services.Configure<MongoConfig>(builder.Configuration.GetSection("Mongo"));
    builder.Services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
    
    builder.Services.AddHealthChecks();
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
    
    // Set up the endpoints and their dependencies
    builder.Services.AddSingleton<IExamplePersistence, ExamplePersistence>();
    builder.Services.AddSingleton<IAtomHistoryService, AtomHistoryService>();
}

[ExcludeFromCodeCoverage]
static WebApplication SetupApplication(WebApplication app)
{
    app.UseHeaderPropagation();
    app.UseRouting();
    app.MapHealthChecks("/health");

    // Example module, remove before deploying!
    app.UseExampleEndpoints();
    app.UseServiceAtomHistoryEndpoints();
    app.UseHangfireDashboard("/hangfire");
    return app;
}
