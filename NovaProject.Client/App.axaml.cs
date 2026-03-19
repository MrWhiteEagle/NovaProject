using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NovaProject.Core.Infrastructure.ClientServices;
using NovaProject.Core.Services;
using NovaProject.Views;

namespace NovaProject;

public partial class App : Application
{
    private MainWindow _mainWindow;
    private DaemonBridgeService _daemonBridge;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        StartupDaemon();
        _daemonBridge = new DaemonBridgeService();
    }

    private void StartupDaemon()
    {
        var currentPid = Process.GetCurrentProcess().Id;
    
        // Find all processes with the Daemon name
        var existing = Process.GetProcessesByName("NovaProject.Daemon")
            .Where(p => p.Id != currentPid) // Safety check
            .ToList();

        if (existing.Count > 0)
        {
            Logger.LogInfo($"Found {existing.Count} ghost processes. Purging...");
            foreach (var p in existing)
            {
                try
                {
                    p.Kill();
                    p.WaitForExit(1000);
                }
                catch(Exception e)
                {
                    Logger.LogError(e.ToString());
                }
            }
        }
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string daemonName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
            ? "NovaProject.Daemon.exe"
            : "NovaProject.Daemon";
        string daemonPath = Path.Combine(baseDir, "Daemon", daemonName);
        if (File.Exists(daemonPath))
        {
            Logger.LogInfo("Starting Daemon server process...");
            LaunchDaemonBinary(daemonPath);
        }
        else
        {
            Logger.LogInfo("Daemon not found at " + daemonPath);
        }
            
        
    }

    private void LaunchDaemonBinary(string daemonPath)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = daemonPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            }
        );
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