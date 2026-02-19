namespace NovaProject.Models;

public class UserListDisplayItem(string name, string displayName)
{
    public string Name { get; set; } = name;
    public string DisplayName { get; set; } = displayName;
}