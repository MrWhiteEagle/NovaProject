namespace NovaProject.Models;

public class User : UserListDisplayItem
{
    public User(string name, string displayName, string tag) : base(name, displayName)
    {
        _tag = tag;
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
}