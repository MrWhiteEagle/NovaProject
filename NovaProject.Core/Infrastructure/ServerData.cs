
namespace NovaProject.Core.Infrastructure;

public class ServerData : UserListDisplayItem
{
    public ServerData(string name, string displayName, string address, string password) : base(name, displayName)
    {
        this.Address = address;
        this.Password = password;
    }
    public string Address { get; set; }
    public string Password { get; set; }
}