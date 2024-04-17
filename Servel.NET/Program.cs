using Servel.NET;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using System.Net;
#if !DEBUG
using Microsoft.Extensions.FileProviders;
using System.Reflection;
#endif

var configuration = ServelConfiguration.Configure();
var sites = configuration.Sites;

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

void BindSite(KestrelServerOptions options, SiteConfiguration site)
{
    void Configure(ListenOptions listenOptions)
    {
        if (site.Certificate != null) listenOptions.UseHttps(site.Certificate);
        listenOptions.Use((context, next) =>
        {
            context.Items.Add("siteId", site.Id);
            return next(context);
        });
    }

    if (IPAddress.TryParse(site.Host, out var ipAddress)) options.Listen(ipAddress, site.Port, Configure);
    else if (site.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) options.ListenLocalhost(site.Port, Configure);
    else options.ListenAnyIP(site.Port, Configure);
}

builder.WebHost.UseKestrel(serverOptions =>
{
    foreach(var site in sites) BindSite(serverOptions, site);
});

// Add services to the container.
builder.Services.AddMemoryCache();

var app = builder.Build();

#if DEBUG
var provider = app.Environment.WebRootFileProvider;
#else
var provider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "Assets");
#endif

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

void ConfigureSite(IApplicationBuilder app, SiteConfiguration site)
{
    if (!site.AllowPublicAccess) app.UseMiddleware<DenyPublicAccessMiddleware>();

    if (site.Credentials.HasValue) app.UseMiddleware<BasicAuthenticationMiddleware>(site.Credentials.Value);

    // Configure the HTTP request pipeline.

    //app.UseHttpsRedirection();

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = provider,
        RequestPath = "/_servel",
    });

    foreach (var listing in site.Listings) Mount(app, listing);

    if (!site.Listings.Any(l => l.IsMountAtRoot)) app.UseMiddleware<HomeMiddleware>(site.Listings);
}

foreach(var site in sites) app.MapWhen(context => context.SiteId() == site.Id, siteApp => ConfigureSite(siteApp, site));

app.Run();
