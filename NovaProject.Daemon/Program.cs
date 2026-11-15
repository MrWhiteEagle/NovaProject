using NovaProject.Core.Services;
using NovaProject.Daemon.Database;
using NovaProject.Daemon.Services;
using NovaProject.Daemon.Services.Database;
using SQLitePCL;

namespace NovaProject.Daemon;

public class Program
{
    private static readonly Mutex SingleInstanceMutex = new Mutex(true, "NovaApp.Daemon.Lock");
    [STAThread]
    public static async Task Main(string[] args)
    {
        SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlcipher());
        
        if (!SingleInstanceMutex.WaitOne(TimeSpan.Zero, true))
        {
            Console.WriteLine("[DAEMON] Another instance already running. Exiting.");
            return;
        }

        try
        {
            Console.WriteLine("=== NovaApp.Daemon bootstrapping ===");
            var builder = Host.CreateApplicationBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NovaProject");
            Directory.CreateDirectory(appData);

            string registryPath = Path.Combine(appData, "nova_profiles.db");

            builder.Services.AddSingleton(new MasterRegistryDbContext(registryPath));
            builder.Services.AddSingleton(new UserVaultManager(appData));
            builder.Services.AddSingleton<UserProfileService>();
            builder.Services.AddSingleton<LocalVaultDataService>();

            builder.Services.AddHostedService<DaemonWorker>();

            var host = builder.Build();
            Console.WriteLine("=== NovaApp.Daemon Running (Ctrl+C to terminate)  ===");

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[DAEMON][FATAL] Daemon Terminated: {ex.Message}");
            Console.ResetColor();
        }
        finally
        {
            SingleInstanceMutex.ReleaseMutex();
            SingleInstanceMutex.Dispose();
        }
    }
}