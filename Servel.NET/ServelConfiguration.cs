﻿using System.Web;

namespace Servel.NET;

public readonly record struct ServelConfiguration(Site[] Sites)
{
    public static ServelConfiguration Configure() => Configure(AppContext.BaseDirectory);

    public static ServelConfiguration Configure(string basePath)
    {
        var configPath = Path.Combine(basePath, "Configuration.toml");
        EnsureConfigurationFile(configPath);

        var configurator = new ServelConfigurator(basePath);
        return configurator.Configure(configPath);
    }

    private static void EnsureConfigurationFile(string configPath)
    {
        if (File.Exists(configPath)) return;

        var password = HttpUtility.JavaScriptStringEncode(KeyGenerator.GetUniqueKey(20));
        var publicPath = Path.GetFullPath("..\\", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
        var encodedPublicPath = HttpUtility.JavaScriptStringEncode(publicPath);

        var defaultConfiguration = StaticResources.Get("DefaultConfiguration.toml");
        var renderedConfiguration = string.Format(defaultConfiguration, password, encodedPublicPath);
        File.WriteAllText(configPath, renderedConfiguration);
    }
}
