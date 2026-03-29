using NovaProject.Core.Infrastructure.Structs;

namespace NovaProject.Core.Infrastructure;

public class MessageOutgoing(string content, LocalUid sender, LocalUid recipient) : MessageIo(content, sender, recipient)
{
    
}