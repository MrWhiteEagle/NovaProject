using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Core.Infrastructure;

public class User : UserListDisplayItem
{
    public User(string name, string displayName, string tag, string relayAddress="") : base(name, displayName)
    {
        _tag = tag;
        _relayAddress = relayAddress;
    }

    private string _tag;
    public string Tag
    {
        get => _tag;
        set {
            _tag = value;
            OnPropertyChanged();
        } 
    }
    
    private string _relayAddress;

    public string RelayAddress
    {
        get => _relayAddress;
        set {
            _relayAddress = value;
            OnPropertyChanged();
        }
    }

    public LocalUid ToLocalUid() => new LocalUid(this.Name, this.DisplayName,  this.Tag,  this.RelayAddress);
}