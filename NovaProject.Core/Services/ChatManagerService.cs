using System.Collections.ObjectModel;
using NovaProject.Core.Infrastructure;

namespace NovaProject.Core.Services;

public class ChatManagerService
{
    public ObservableCollection<ServerInstance> Servers { get; } = new();

    public async Task AddServer(string url)
    {
        var server = new ServerInstance(url);
        Servers.Add(server);
        await server.EnsureConnected();
    }

    public async Task SwitchToServer(ServerInstance server)
    {
        await server.EnsureConnected();
    }
}