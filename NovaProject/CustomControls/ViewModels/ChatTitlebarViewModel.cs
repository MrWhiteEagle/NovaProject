using CommunityToolkit.Mvvm.ComponentModel;
using NovaProject.Models;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public partial class ChatTitlebarViewModel : ViewModelBase
{
    [ObservableProperty] private User _userContext;

    public void ChangeUserContext(User user)
    {
        UserContext = user;
    }
}