using CommunityToolkit.Mvvm.ComponentModel;
using NovaProject.Core.Infrastructure;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public partial class ChatTitlebarViewModel : ViewModelBase
{
    [ObservableProperty] private User? _userContext;

    public void ChangeUserContext(User user)
    {
        UserContext = user;
    }
}