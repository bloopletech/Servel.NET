using Servel.NET;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using idunno.Authentication.Basic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Principal;

await InstallerManagement.ProcessArguments(args);

var configuration = ServelConfiguration.Parse();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "Assets"
});

builder.Host.UseWindowsService();

builder.WebHost.UseKestrel((serverOptions) =>
{
    var configure = (ListenOptions listenOptions) =>
    {
        if (configuration.Certificate != null) listenOptions.UseHttps(configuration.Certificate);
    };

    if (configuration.Host != null) serverOptions.Listen(configuration.Host, configuration.Port, configure);
    else serverOptions.ListenAnyIP(configuration.Port, configure);

    //serverOptions.Listen(configuration.Host, configuration.Port, listenOptions =>
    //{

    //});
});

// Add services to the container.
builder.Services.AddSingleton(configuration);

if(configuration.Username != null)
{
    builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
        .AddBasic(options =>
        {
            options.Realm = "Servel.NET";
            options.Events = new BasicAuthenticationEvents
            {
                OnValidateCredentials = context =>
                {
                    if (context.Username == configuration.Username && context.Password == configuration.Password)
                    {
                        context.Principal = new ClaimsPrincipal(new GenericIdentity("DefaultUser"));
                        context.Success();
                    }

                    return Task.CompletedTask;
                }
            };
        });
    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    });
}

builder.Services.AddFluid();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (configuration.Username != null)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = app.Environment.WebRootFileProvider,
    RequestPath = "/_servel"
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
