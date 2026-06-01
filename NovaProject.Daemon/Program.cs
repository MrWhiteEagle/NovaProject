using NovaProject.Core.Services;
using NovaProject.Daemon.Services;

namespace NovaProject.Daemon;

public class Program
{
    private static Mutex _mutex = new Mutex(true, "NovaProject.Daemon.Lock");
    [STAThread]
    public static void Main(string[] args)
    {
        if (!_mutex.WaitOne(TimeSpan.Zero, true))
        {
            return;
        }

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<DaemonWorker>();
            });
}