using System.Web;

namespace Servel.NET;

public static class ServelConfigurationProvider
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
        string publicPath;
        if(Environment.IsPrivilegedProcess)
        {
            publicPath = Path.GetFullPath(
                "..\\",
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
        }
        else
        {
            publicPath = ".";
        }
        var encodedPublicPath = HttpUtility.JavaScriptStringEncode(publicPath);

        var defaultConfiguration = Resources.Get("DefaultConfiguration.toml");
        var renderedConfiguration = string.Format(defaultConfiguration, password, encodedPublicPath);
        File.WriteAllText(configPath, renderedConfiguration);
    }
}
