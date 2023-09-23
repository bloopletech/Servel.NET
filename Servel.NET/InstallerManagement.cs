using CliWrap;
using System.Data;
using System;
using System.Diagnostics;

namespace Servel.NET;
public static class InstallerManagement
{
    public static async Task ProcessArguments(string[] args)
    {
        const string ServiceName = "Servel.NET";

        if (args is { Length: 1 })
        {
            try
            {
                string executablePath = Path.Combine(AppContext.BaseDirectory, "Servel.NET.exe");

                if (args[0] is "/Install")
                {

                    await Cli.Wrap("netsh")
                        .WithArguments(new[] { "advfirewall", "firewall", "add", "rule", $"name={ServiceName}", "dir=in", "action=allow", $"program={executablePath}", "profile=Private", "enable=yes" })
                        .ExecuteAsync();

                    await Cli.Wrap("sc")
                        .WithArguments(new[] { "create", ServiceName, $"binPath={executablePath}", "start=auto" })
                        .ExecuteAsync();

                    await Cli.Wrap("sc")
                        .WithArguments(new[] { "description", ServiceName, "Serves directories on your computer to your local network over HTTP/HTTPS." })
                        .ExecuteAsync();

                    await Cli.Wrap("sc")
                        .WithArguments(new[] { "start", ServiceName })
                        .ExecuteAsync();
                }
                else if (args[0] is "/Uninstall")
                {
                    await Cli.Wrap("sc")
                        .WithArguments(new[] { "stop", ServiceName })
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteAsync();

                    await Cli.Wrap("sc")
                        .WithArguments(new[] { "delete", ServiceName })
                        .ExecuteAsync();

                    await Cli.Wrap("netsh")
                        .WithArguments(new[] { "advfirewall", "firewall", "delete", "rule", $"name={ServiceName}", $"program={executablePath}" })
                        .ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Environment.Exit(0);
        }
    }
}
