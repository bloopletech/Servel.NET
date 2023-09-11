using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using Servel.NET;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using idunno.Authentication.Basic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Principal;

var yamlBasePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Servel.NET");
var ymalPath = Path.Combine(yamlBasePath, "servel.yml");

var yamlDeserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
var configurationYaml = yamlDeserializer.Deserialize<ConfigurationYaml>(File.ReadAllText(ymalPath));

var configuration = new ServelConfiguration(configurationYaml, yamlBasePath);


var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "Assets"
});

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

if(configuration.Listings.Count() > 1)
{
    foreach (var listing in configuration.Listings)
    {
        app.Map(listing.RequestPath, false, app =>
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = listing.FileProvider,
                ServeUnknownFileTypes = true
            });
            app.UseMiddleware<IndexMiddleware>(listing);
        });
    }

    app.UseMiddleware<HomeMiddleware>(configuration.Listings);
}
else if(configuration.Listings.Count() == 1)
{
    var listing = configuration.Listings.First();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = listing.FileProvider,
        ServeUnknownFileTypes = true
    });
    app.UseMiddleware<IndexMiddleware>(listing);
}



app.Run();
