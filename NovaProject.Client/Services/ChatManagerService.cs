using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NovaProject.Core.Infrastructure;
using NovaProject.Models;

namespace NovaProject.Services;

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