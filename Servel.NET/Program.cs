using Servel.NET;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Connections;
using System.Net;
using Servel.NET.Extensions;
using Servel.NET.Services;
using CliWrap;

var configuration = ServelConfigurationProvider.Configure();
var sites = configuration.Sites;

var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Production,
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

void BindSite(KestrelServerOptions options, Site site)
{
    void Configure(ListenOptions listenOptions)
    {
        if(site.Certificate != null) listenOptions.UseHttps(site.Certificate);
        listenOptions.Use((context, next) =>
        {
            context.Items.Add("siteId", site.Id);
            return next(context);
        });
    }

    if(IPAddress.TryParse(site.Host, out var ipAddress)) options.Listen(ipAddress, site.Port, Configure);
    else if(site.Host.EqualsIgnoreCase("localhost")) options.ListenLocalhost(site.Port, Configure);
    else options.ListenAnyIP(site.Port, Configure);
}

builder.WebHost.UseKestrel(serverOptions =>
{
    foreach(var site in sites) BindSite(serverOptions, site);
});

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

if(configuration.DatabasePath != null)
{
    builder.Services.AddSingleton(new DatabaseService(configuration.DatabasePath));
    builder.Services.AddSingleton<HistoryService>();
}

if(configuration.CacheDatabasePath != null)
{
    builder.Services.AddSingleton(new CacheDatabaseService(configuration.CacheDatabasePath));
    builder.Services.AddSingleton<ThumbnailService>();
}

var app = builder.Build();

app.UseExceptionHandler(app => app.Run(context => Task.CompletedTask));
if(app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

if(configuration.DatabasePath != null || configuration.CacheDatabasePath != null)
{
    NativeLibraries.Init();
    app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(NativeLibraries.OnShutdown);
}

app.Services.GetService<DatabaseService>()?.CreateSchema();
app.Services.GetService<CacheDatabaseService>()?.CreateSchema();

void MountInternal(IApplicationBuilder app, Root root, DirectoryOptionsResolver resolver)
{
    if(configuration.CacheDatabasePath != null) app.UseMiddleware<ThumbnailMiddleware>(root, resolver);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = root.FileProvider,
        ServeUnknownFileTypes = true,
        OnPrepareResponse = HistoryService.OnPrepareResponse
    });
    app.UseMiddleware<IndexMiddleware>(root, resolver);
}

void Mount(IApplicationBuilder app, Root root, DirectoryOptionsResolver resolver)
{
    if(root.IsMountAtRoot) MountInternal(app, root, resolver);
    else app.Map(root.UrlPath, false, app => MountInternal(app, root, resolver));
}

void ConfigureSite(IApplicationBuilder app, Site site)
{
    app.UseMiddleware<DenyAudienceMiddleware>(site.Audience);

    if(site.Credentials.HasValue)
    {
        app.UseMiddleware<BlockFailedAuthenticationMiddleware>();
        if(site.JwtSigningKey != null) app.UseMiddleware<JwtAuthenticationMiddleware>(site);
        app.UseMiddleware<BasicAuthenticationMiddleware>(site.Credentials.Value);
    }

    // Configure the HTTP request pipeline.

    //app.UseHttpsRedirection();

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = Resources.AssetsFileProvider,
        RequestPath = "/_servel",
    });

    if(site.JwtSigningKey != null) app.UseMiddleware<GenerateSignedUrlMiddleware>(site);

    var resolver = new DirectoryOptionsResolver(site.DirectoriesOptions);
    foreach(var root in site.Roots) Mount(app, root, resolver);

    if(!site.Roots.Any(l => l.IsMountAtRoot)) app.UseMiddleware<HomeMiddleware>(site.Roots);
}

foreach(var site in sites) app.MapWhen(context => context.SiteId() == site.Id, siteApp => ConfigureSite(siteApp, site));

app.Run();
