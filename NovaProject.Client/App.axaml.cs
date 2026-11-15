using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Client.ViewModels;
using NovaProject.Client.Views;
using NovaProject.Core.Services;

namespace NovaProject.Client;

public partial class App : Application
{
    private MainWindow _mainWindow;
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }
        var collection = new ServiceCollection();
        collection.AddSingleton<DaemonBridgeService>();
        collection.AddSingleton<ChatService>();
        collection.AddSingleton<NotificationService>();

        collection.AddTransient<MainWindowViewModel>();
        
        ServiceProvider = collection.BuildServiceProvider();
        
        var bridge = ServiceProvider.GetRequiredService<DaemonBridgeService>();
        bridge.Start();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _mainWindow = new MainWindow()
            {
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>(),
            };
            desktop.MainWindow = _mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    public void TrayMenu_OpenApp(object? sender, EventArgs e)
    {
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
    }

    public void TrayMenu_CloseApp(object? sender, EventArgs e)
    {
        _mainWindow.IsReallyClosing = true;
        _mainWindow.Close();
    }

    public void TrayMenu_MinimizeApp(object? sender, EventArgs e)
    {
        _mainWindow.Hide();
        _mainWindow.WindowState = WindowState.Minimized;
    }
}