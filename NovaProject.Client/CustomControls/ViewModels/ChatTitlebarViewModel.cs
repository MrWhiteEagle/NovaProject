using CommunityToolkit.Mvvm.ComponentModel;
using NovaProject.Core.Infrastructure;
using NovaProject.Core.Infrastructure.Structs;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public partial class ChatTitlebarViewModel : ViewModelBase
{
    [ObservableProperty] private LocalUid? _userContext;

    public void ChangeUserContext(LocalUid user)
    {
        UserContext = user;
    }
}