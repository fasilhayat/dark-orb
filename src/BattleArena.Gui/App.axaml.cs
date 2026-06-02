using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using BattleArena.Gui.Views;

namespace BattleArena.Gui;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                Console.WriteLine("App: creating MainWindow...");
                var mainWindow = new MainWindow();
                Console.WriteLine("App: setting MainWindow...");
                desktop.MainWindow = mainWindow;
                Console.WriteLine("App: done");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"App: OnFrameworkInitializationCompleted FAILED: {ex}");
                throw;
            }
            base.OnFrameworkInitializationCompleted();
        }
    }
}
