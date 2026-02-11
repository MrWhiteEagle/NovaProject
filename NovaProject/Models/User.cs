namespace NovaProject.Models;

public class User(string uname, string tag)
{
    public string Username { get; set; } = uname;
    public string Tag { get; set; } = tag;
}