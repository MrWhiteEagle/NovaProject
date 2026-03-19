using DesktopNotifications;
using DesktopNotifications.FreeDesktop;
using DesktopNotifications.Windows;
using INotificationManager = DesktopNotifications.INotificationManager;

namespace NovaProject.Core.Services;

public class NotificationService
{
    private INotificationManager _notificationManager = OperatingSystem.IsLinux() 
        ? new FreeDesktopNotificationManager() 
        : new WindowsNotificationManager();

    public NotificationService()
    {
        Initialize();
    }

    private async void Initialize()
    {
        try
        {
            await _notificationManager.Initialize();
        }
        catch
        {
            Logger.LogError("Initialization Failed");
            throw new Exception("There was a problem initializing the notification service.");
        }
        finally
        {
            Logger.LogInfo("The notification service has been initialized.");
        }
    }

    public async void ShowNotification(Notification notification)
    {
        try
        {
            await _notificationManager.ShowNotification(notification, DateTimeOffset.Now.AddSeconds(AppGlobalService.MessageNotificationTime));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
        }
    }

    public async void ShowNotification(string title, string message)
    {
        var notification = new Notification
        {
            Title = title,
            Body = message,
        };
        try
        {
            await _notificationManager.ShowNotification(
                new Notification
            {
                Title = title,
                Body = message,
            }, 
                expirationTime: DateTimeOffset.Now.AddSeconds(AppGlobalService.MessageNotificationTime));

        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
        }
    }
}