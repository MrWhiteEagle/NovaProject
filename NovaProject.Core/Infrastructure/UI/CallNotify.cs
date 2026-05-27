using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Core.Infrastructure.Ui;

public class CallNotifyOutgoing(LocalUid sender, LocalUid recipient) : MessageIo("", sender, recipient)
{
    
}

public class CallNotifyIncoming(LocalUid sender, LocalUid recipient) : MessageIo("", sender, recipient)
{
    
}