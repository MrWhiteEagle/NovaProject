using CommunityToolkit.Mvvm.ComponentModel;
using NovaProject.Client.ViewModels;
using NovaProject.Core.Infrastructure.Local;

namespace NovaProject.Client.CustomControls.ViewModels;

public partial class ChatTitlebarViewModel : ViewModelBase
{
    [ObservableProperty] private LocalUid? _userContext;

    public void ChangeUserContext(LocalUid user)
    {
        UserContext = user;
    }
}