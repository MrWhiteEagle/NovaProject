using NovaProject.Models;
using NovaProject.ViewModels;

namespace NovaProject.CustomControls.ViewModels;

public class ChatTitlebarViewModel(User context) : ViewModelBase
{
    public User UserContext { get; set; } = context;
}