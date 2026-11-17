
namespace NovaProject.Core.Services;

public class NotificationService
{

    public NotificationService()
    {
        Initialize();
    }

    private async void Initialize()
    {
        try
        {
        }
        catch
        {
            NovaLogger.LogError("Initialization Failed");
            throw new Exception("There was a problem initializing the notification service.");
        }
        finally
        {
            NovaLogger.LogInfo("The notification service has been initialized.");
        }
    }

    public async void ShowNotification()
    {
        try
        {
           
        }
        catch (Exception ex)
        {
            NovaLogger.LogError(ex.Message);
        }
    }

    public async void ShowNotification(string title, string message)
    {
        
        try
        {
            

        }
        catch (Exception ex)
        {
            NovaLogger.LogError(ex.Message);
        }
    }
}