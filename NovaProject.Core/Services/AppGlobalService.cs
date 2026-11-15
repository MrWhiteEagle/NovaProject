using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Data.Shared;
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Core.Services;

public static class AppGlobalService
{
    public static LocalUid CurrentUser { get; private set; }

    public static int MessageNotificationTime = 5;
    public static int MessageHistoryLimit = 100;

    public static void SetCurrentUser(LocalUid user)
    {
        CurrentUser = user;
    }
}