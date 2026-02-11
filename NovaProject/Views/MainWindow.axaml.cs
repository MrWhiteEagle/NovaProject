using Avalonia.Controls;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

namespace NovaProject.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ChatField_OnMessageSent(object? sender, MessageSentEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        vm?.UpdateMessagesRequest(e);
    }
}