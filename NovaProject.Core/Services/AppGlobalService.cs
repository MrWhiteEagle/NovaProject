using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Core.Services;

public static class AppGlobalService
{
    public static User? CurrentUser { get; private set; }
    public static LocalUid? LocalUid { get; private set; }

    public static int MessageNotificationTime = 5;

    public static void SetCurrentUser(User user)
    {
        CurrentUser = user;
        LocalUid = new LocalUid(user.Name, user.DisplayName, user.Tag, CurrentUser.RelayAddress);
    }
}