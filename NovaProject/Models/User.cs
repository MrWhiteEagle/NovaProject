namespace NovaProject.Models;

public class User
{
    public string Username { get; set; }
    public string Tag { get; set; }

    public User(string uname, string tag)
    {
        this.Username = uname;
        this.Tag = tag;
    }
    
}