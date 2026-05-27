using System.Text.Json.Serialization;

namespace NovaProject.Core.Infrastructure.Local;

public record LocalUid(
    [property: JsonPropertyName("Name")] string Name, 
    [property: JsonPropertyName("UserName")] string UserName, 
    [property: JsonPropertyName("Tag")] string Tag, 
    [property: JsonPropertyName("RelayAddress")] string RelayAddress
) : IEquatable<User>
{
    
    public virtual bool Equals(User? other)
    {
        return Name ==  other?.Name && Tag ==  other?.Tag && RelayAddress ==  other?.RelayAddress;
    }

    public virtual bool Equals(LocalUid other)
    {
        return Name == other?.Name && Tag == other.Tag && RelayAddress == other.RelayAddress;
    }

    public override string ToString() => $"[{Name}#{Tag}@{RelayAddress};]";
    public User ToUser() => new User(this.Name, this.UserName, this.Tag, this.RelayAddress);

}