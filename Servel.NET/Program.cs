using Servel.NET;
using Microsoft.AspNetCore.Server.Kestrel.Core;
#if !DEBUG
using Microsoft.Extensions.FileProviders;
using System.Reflection;
#endif

var configuration = ServelConfiguration.Configure();

var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production,
#if DEBUG
    WebRootPath = "Assets"
#endif
});

builder.Services.AddLogging();
builder.Logging.AddSimpleConsole(options => options.IncludeScopes = true);
builder.Logging.AddDebug();
builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions =
        ActivityTrackingOptions.SpanId |
        ActivityTrackingOptions.TraceId |
        ActivityTrackingOptions.ParentId;
});
builder.Logging.SetMinimumLevel(builder.Environment.IsDevelopment() ? LogLevel.Trace : LogLevel.Warning);

builder.Host.UseWindowsService();

builder.WebHost.UseKestrel((serverOptions) =>
{
    void configure(ListenOptions listenOptions)
    {
        if (configuration.Certificate != null) listenOptions.UseHttps(configuration.Certificate);
    }

    if (configuration.Host != null) serverOptions.Listen(configuration.Host, configuration.Port, configure);
    else serverOptions.ListenAnyIP(configuration.Port, configure);

    //serverOptions.Listen(configuration.Host, configuration.Port, listenOptions =>
    //{

    //});
});

// Add services to the container.
builder.Services.AddSingleton(configuration);

builder.Services.AddMemoryCache();

var app = builder.Build();

if (!configuration.AllowPublicAccess) app.UseMiddleware<DenyPublicAccessMiddleware>();

if (configuration.Credentials.HasValue) app.UseMiddleware<BasicAuthenticationMiddleware>(configuration.Credentials.Value);

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

#if DEBUG
var provider = app.Environment.WebRootFileProvider;
#else
var provider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "Assets");
#endif

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = provider,
    RequestPath = "/_servel",
});

void MountInternal(IApplicationBuilder app, Listing listing)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = listing.FileProvider,
        ServeUnknownFileTypes = true
    });
    app.UseMiddleware<IndexMiddleware>(listing);
}

void Mount(IApplicationBuilder app, Listing listing)
{
    if (listing.IsMountAtRoot) MountInternal(app, listing);
    else app.Map(listing.RequestPath, false, app => MountInternal(app, listing));
}

foreach (var listing in configuration.Listings) Mount(app, listing);

if(!configuration.Listings.Any(l => l.IsMountAtRoot)) app.UseMiddleware<HomeMiddleware>(configuration.Listings);

app.Run();
