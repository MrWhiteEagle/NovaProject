
namespace NovaProject.Core.Infrastructure.Data.Shared;

public class ServerData(string name, string displayName, string address)
    : UserListDisplayItem(name, displayName, "0000", address)
{
    public string Address { get; set; } = address;
}