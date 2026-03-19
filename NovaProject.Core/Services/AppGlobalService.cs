using System;
using System.Runtime.CompilerServices;
using NovaProject.Core.Infrastructure;

namespace NovaProject.Core.Services;

public static class AppGlobalService
{
    public static User? CurrentUser;
    public static int MessageNotificationTime = 5;

    public static void SetCurrentUser(User user)
    {
        CurrentUser = user;
    }
}