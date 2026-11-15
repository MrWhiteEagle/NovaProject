using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NovaProject.Core.Infrastructure;

public class UserListDisplayItem : INotifyPropertyChanged
{
    public UserListDisplayItem(string name, string displayName, string tag, string relayAddress = "")
    {
        _name = name;
        _displayName = displayName;
        _tag = tag;
        _relayAddress = relayAddress;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private string _name;

    public string Name
    {
        get => _name; 
        set { _name = value; OnPropertyChanged(); }
    }
    private string _displayName;

    public string DisplayName
    {
        get => _displayName; 
        set { _displayName = value; OnPropertyChanged(); }
    }
    
    private string _relayAddress;

    public string RelayAddress
    {
        get => _relayAddress;
        set { _relayAddress = value; OnPropertyChanged(); }
    }

    private string _tag;

    public string Tag
    {
        get => _tag;
        set { _tag = value; OnPropertyChanged(); }
    }

    private int _unreadCount;

    public int UnreadCount
    {
        get => _unreadCount;
        set { _unreadCount = value; OnPropertyChanged(); }
    }
}