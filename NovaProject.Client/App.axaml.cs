using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NovaProject.Client.Services;
using NovaProject.Client.Views;

namespace NovaProject.Client;

public partial class App : Application
{
    private MainWindow _mainWindow;
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    public override void Initialize()
    {
        var collection = new ServiceCollection();
        collection.AddSingleton<DaemonBridgeService>();
        collection.AddSingleton<ChatService>();
        ServiceProvider = collection.BuildServiceProvider();
        AvaloniaXamlLoader.Load(this);
        //Initial service creation
        _ = ServiceProvider.GetService<DaemonBridgeService>();
        _ = ServiceProvider.GetService<ChatService>();
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            _mainWindow = new MainWindow();
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