using Avalonia.Controls;
using Avalonia.Interactivity;
using NovaProject.Client.ViewModels;

namespace NovaProject.Client.Views;

public partial class MainWindow : Window
{
    public bool IsReallyClosing = false;
    public MainWindow()
    {
        InitializeComponent();
        this.Closing += MainWindow_OnClose;
    }
    
    private void MainWindow_OnClose(object? sender, WindowClosingEventArgs e)
    {
        if (IsReallyClosing) return;
        e.Cancel = true;
        this.Hide();
        this.WindowState = WindowState.Minimized;
    }
}