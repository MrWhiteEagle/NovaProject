using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NovaProject.CustomControls;
using NovaProject.Models.Events;
using NovaProject.ViewModels;

namespace NovaProject.Views;

public partial class MainWindow : Window
{
    MainWindowViewModel vm;
    public MainWindow()
    {
        InitializeComponent();
        vm = new MainWindowViewModel();
        DataContext = vm;
        this.Loaded += MainWindow_OnLoaded;
    }

    private void MainWindow_OnLoaded(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("MainWindow_OnLoaded");
        vm.SetupTabs();
    }

    private void ChatField_OnMessageSent(object? sender, MessageSentEventArgs e)
    {
        vm.UpdateMessagesRequest(e);
    }
}