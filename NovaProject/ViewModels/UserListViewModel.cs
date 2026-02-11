using System.Collections.ObjectModel;
using NovaProject.Models;

namespace NovaProject.ViewModels;

public partial class UserListViewModel : ViewModelBase
{
    
    public static ObservableCollection<User> UserConversations { get; set; } =
    [
        new User("SAMPLE_USER_001", "1111"),
        new User("SAMPLE_USER_002", "2222"),
        new User("SAMPLE_USER_003", "3333"),
        new User("SAMPLE_USER_004", "4444"),
        new User("SAMPLE_USER_005", "5555"),
        new User("SAMPLE_USER_006", "6666"),
        new User("SAMPLE_USER_007", "7777"),
        new User("SAMPLE_USER_008", "8888"),
        new User("SAMPLE_USER_009", "9999"),
        new User("SAMPLE_USER_010", "1010"),
    ];
}