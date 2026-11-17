using System.Text.Json.Serialization;

namespace NovaProject.Core.Infrastructure.Data.Shared;

public record LocalUid(
    [property: JsonPropertyName("Name")] string Name, 
    [property: JsonPropertyName("UserName")] string UserName, 
    [property: JsonPropertyName("Tag")] string Tag, 
    [property: JsonPropertyName("RelayAddress")] string RelayAddress
)
{

    public virtual bool Equals(LocalUid? other)
    {
        return this.ToString() == other?.ToString();
    }

    public override string ToString() => $"[{Name}#{Tag}@{RelayAddress};]";

    public static LocalUid Parse(string input)
    {
        char[] array = input.ToCharArray();
        string name = "";
        string tag = "";
        string relayAddress = "";
        int i = 1;
        while (i < input.Length-1)
        {
            while (array[i] != '#')
            {
                name += array[i];
                i++;
            }
            i++;
            while (array[i] != '@')
            {
                tag += array[i];
                i++;
            }
            i++;
            while (array[i] != ';')
            {
                relayAddress += array[i];
                i++;
            }
            break;
        }
        return new LocalUid(name, "", tag, relayAddress);
        
    }

    public UserListDisplayItem ToDisplayItem()
    {
        return new UserListDisplayItem(this.Name, this.UserName, this.Tag, this.RelayAddress);
    }

}