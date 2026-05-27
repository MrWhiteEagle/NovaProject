using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Client.ViewModels;
using NovaProject.Core.Infrastructure.Local;
using NovaProject.Core.Services;

namespace NovaProject.Client.CustomControls.ViewModels;

public partial class ChatInputViewModel : ViewModelBase
{
    private LocalUid? _userContext;
    private string _inputText = string.Empty;
    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    [RelayCommand]
    private void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(InputText)) return;
        var message = new MessageData(InputText, AppGlobalService.LocalUid, _userContext);
        App.ServiceProvider.GetRequiredService<ChatService>().SendMessage(message);
        _inputText = string.Empty;
    }

    public void ChangeUserContext(LocalUid? userContext)
    {
        Logger.LogInfo("Changing user context to " + userContext.UserName);
        this._userContext = userContext;
    }
}