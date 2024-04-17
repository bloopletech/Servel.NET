using System.Text.Json;
using System.Web;

namespace Servel.NET;

public readonly record struct ServelConfiguration(IEnumerable<SiteConfiguration> Sites)
{
    public static ServelConfiguration Configure() => Configure(AppContext.BaseDirectory);

    public static ServelConfiguration Configure(string basePath)
    {
        var configPath = Path.Combine(basePath, "Configuration.json");
        EnsureConfigurationFile(configPath);

        using var inputStream = File.OpenRead(configPath);
        var sitesOptions = JsonSerializer.Deserialize(
            inputStream,
            DeserializationSourceGenerationContext.Default.SiteOptionsArray)!;

        var sites = sitesOptions.Select((siteOptions, id) => new SiteConfiguration(id, siteOptions, basePath));
        return new ServelConfiguration(sites);
    }

    private static void EnsureConfigurationFile(string configPath)
    {
        if (File.Exists(configPath)) return;

        var password = HttpUtility.JavaScriptStringEncode(KeyGenerator.GetUniqueKey(20));
        var publicPath = Path.GetFullPath("..\\", Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
        var encodedPublicPath = HttpUtility.JavaScriptStringEncode(publicPath);

        File.WriteAllText(configPath, $$"""
            /*
            This file contains examples of the options that Servel.NET supports;
            uncomment, comment out, and modify the examples as needed to achieve your goals.
            */
            [
              {
                /*
                Host is the IP address for Servel.NET to bind to and serve your listings at.
                By default Servel.NET will bind to all interfaces (both IP v4 and IP v6, if available).
                */
                //"Host": "*",
                //"Host": 192.168.1.5",

                /*
                Port is the TCP port for Servel.NET to bind to and serve your listings at.
                By default Servel.NET will bind to port 80.
                */
                "Port": 9292,
                //"Port": 12345,

                /*
                Cert and Key allow you to enable TLS/HTTPS support in Servel.NET.

                If you configure this then HTTP access will be disabled, and clients/users can only connect using HTTPS.

                Note that this is independent of the Port; that is, if you configure Cert and Key, HTTPS will be enabled
                over the Port you specify, even if that port is not 443.
                If you use a non-standard port in combination with HTTPS, then you may have to manually type https:// in your browser
                at the start of the URL to be able to access Servel.NET.

                If you want to enable TLS/HTTPS support, you have to provide both Cert and Key.

                Cert is a filesystem path of a PEM-encoded TLS certificate public key.
                Key is a filesystem path of a PEM-encoded TLS certificate private key.

                You can use relative paths; they will be resolved relative to the folder where Servel.NET.exe is located.
                When entering the filesystem path, you will need to add an extra backslash before each backslash;
                for example, C:\Temp\ becomes C:\\Temp\\ .
                */
                //"Cert": "servel.crt",
                //"Key": "servel.key",

                /*
                Username and Password allow you to enable HTTP Basic Authentication support in Servel.NET.

                If you configure this then anonymous access will be disabled, and clients/users can only connect
                by providing the specified credentials.

                Note that unless you *also* enable TLS/HTTPS support, HTTP Basic Authentication should be considered insecure,
                as the credentials are effectively transmitted in plain text from client to server; this allows anyone
                connected to the same network as the user or the server at the same time the user accesses Servel.NET
                to steal the credentials.

                If you want to enable HTTP Basic Authentication support, you have to provide both Username and Password.

                The Username and Password you specify here is the exact username and password the user has to type in to authenticate.
                There is no suport for multiple user accounts, or multiple valid usernames and passwords.
                */
                //"Username": "servel",
                //"Password": "{{password}}",

                /*
                By default Servel.NET only allows users/clients to connect if the clients IP address is in the private ranges.
                This is intended to only allow users on the local network to access Servel.NET.

                Unless you misconfigure your network or forward traffic from the public internet
                to the machine hosting Servel.NET in such a way that the source IP of requests is a private address,
                then only users on your local network will be able to connect to Servel.NET.

                Even if the user knows the correct username and password, they still won't be able to connect
                unless they are on the local network.

                AllowPublicAccess enables you to explicitly allow anyone in the public internet to connect to Servel.NET
                and browse and download all the folders and files that are within the drives and folders listed below.
                Of course, even with AllowPublicAccess enabled, you stil have to configure your network to allow access
                from the public internet to the machine hosting Servel.NET for this to actually work.

                Even with AllowPublicAccess enabled, if you have a username and password configured, all users will still
                need to enter the credentials to be able to connect to and access Servel.NET.
                */
                //"AllowPublicAccess": false,

                "Listings": [
                  /*
                  Here's where you say what drives or folders you want Servel.NET to serve (to make available for anyone to access).

                  This is an array of objects. Each object has a single property (key-value pair).
                  The key is the filesystem path of a drive or folder you want to serve.
                  The value is the URL path that drive or folder will be served at.

                  The entrie contents of the drive or folder, inluding all the folders and files within that drive or folder,
                  will be available for any user to browse and download.

                  When entering the filesystem path, you will need to add an extra backslash before each backslash;
                  for example, C:\Temp\ becomes C:\\Temp\\ .
                  When entering the URL path, use forward slashes; you don't need to double them up, that's only for the filesystem path.

                  The filesystem path cannot be a relative path; it must be an absolute path.

                  If you only put in a single object (only one key-value pair overall), and use "/" for the URL path,
                  then the drive or folder will be served directly as the root URL.

                  If you put in multiple objects (multiple key-value pairs overall),
                  then each one needs to have a different value for the URL path.
                  When the user accesses the root URL, they will be shown a list of all the URL paths,
                  and the user can easily navigate to each drive or folder by clicking the corresponding URL path.

                  By default the Windows Public folder will be served directly at the root URL.
                  You can remove this entry, keep it, or mix and match with the following examples.
                  */
                  {
                    "{{encodedPublicPath}}": "/",
                    // "C:\\": "/c",
                    // "D:\\": "/bluray_drive",
                    // "C:\\Users\\Mum\\Documents": "/mums_documents",
                  }
                ]
              },

              /*
              Servel.NET supports one or more "sites".
              This example configuration contains a single site, but if you uncomment the following block, then a 2nd
              site will be enabled.

              Each site makes available a set of listings over a specified host, port, and scheme (HTTP/HTTPS) combination.
              That is, if you want to have more than one site, each site has to have a different value for Host, Port, or scheme.
              You set scheme by enabling or disabling TLS/HTTPS support.

              You can have a single site and in that single site make several listings available to browse.
              You can also have multiple sites and make a different set of listings available in each site.
              Each site has its own configuration for Username, Password, AllowPublicAccess, etc.
              That is, you can have one site what requires authentication, and another site that doesn't have authentication.
              You could make sensitive data available in the site with authentication,
              and "public" data in the site without authentication.
              */
              /*
              {
                "Port": 9293,
                "Listings": [
                  {
                    // "E:\\": "/e",
                  }
                ]
              }
              */
            ]
            """);
    }
}
