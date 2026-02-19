namespace NovaProject.Models;

public class User(string name, string displayName, string tag) : UserListDisplayItem(name, displayName)
{
    public string Tag { get; set; } = tag;
}