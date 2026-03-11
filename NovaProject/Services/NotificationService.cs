using System;
using DesktopNotifications;
using DesktopNotifications.FreeDesktop;
using DesktopNotifications.Windows;
using INotificationManager = DesktopNotifications.INotificationManager;

namespace NovaProject.Services;

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
            throw new Exception("There was a problem initializing the notification service.");
        }
        finally
        {
            Console.WriteLine("The notification service has been initialized.");
        }
    }

    public async void ShowNotification(Notification notification)
    {
        try
        {
            await _notificationManager.ShowNotification(notification);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async void ShowNotification(string title, string message)
    {
        try
        {
            await _notificationManager.ShowNotification(new Notification
            {
                Title = title,
                Body = message,
            });

        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}