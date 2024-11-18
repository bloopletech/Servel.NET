using Servel.NET;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Connections;
using System.Net;

var configuration = ServelConfiguration.Configure();
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

void MountInternal(IApplicationBuilder app, Listing listing, DirectoryOptionsResolver resolver)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = listing.FileProvider,
        ServeUnknownFileTypes = true
    });
    app.UseMiddleware<IndexMiddleware>(listing, resolver);
}

void Mount(IApplicationBuilder app, Listing listing, DirectoryOptionsResolver resolver)
{
    if (listing.IsMountAtRoot) MountInternal(app, listing, resolver);
    else app.Map(listing.UrlPath, false, app => MountInternal(app, listing, resolver));
}

void ConfigureSite(IApplicationBuilder app, Site site)
{
    if (!site.AllowNetworkAccess) app.UseMiddleware<DenyNetworkAccessMiddleware>();
    if (!site.AllowPublicAccess) app.UseMiddleware<DenyPublicAccessMiddleware>();

    if (site.Credentials.HasValue) app.UseMiddleware<BasicAuthenticationMiddleware>(site.Credentials.Value);

    // Configure the HTTP request pipeline.

    //app.UseHttpsRedirection();

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = Resources.AssetsFileProvider,
        RequestPath = "/_servel",
    });

    var resolver = new DirectoryOptionsResolver(site.DirectoriesOptions);
    foreach (var listing in site.Listings) Mount(app, listing, resolver);

    if (!site.Listings.Any(l => l.IsMountAtRoot)) app.UseMiddleware<HomeMiddleware>(site.Listings);
}

foreach(var site in sites) app.MapWhen(context => context.SiteId() == site.Id, siteApp => ConfigureSite(siteApp, site));

app.Run();
