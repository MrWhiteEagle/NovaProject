using Avalonia.Controls;
using Avalonia.Interactivity;
using NovaProject.CustomControls;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

namespace NovaProject.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm;
    public bool IsReallyClosing = false;
    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainWindowViewModel();
        DataContext = _vm;
        this.Loaded += MainWindow_OnLoaded;
        this.Closing += MainWindow_OnClose;
    }

    #region Events
    private void MainWindow_OnLoaded(object? sender, RoutedEventArgs e)
    {
        _vm.SetupTabs();
        _vm.SetupInitialChatView();
    }

    private void MainWindow_OnClose(object? sender, WindowClosingEventArgs e)
    {
        if (IsReallyClosing) return;
        e.Cancel = true;
        this.Hide();
        this.WindowState = WindowState.Minimized;
    }
    #endregion
}