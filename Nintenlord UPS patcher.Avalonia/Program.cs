using Avalonia;

namespace Nintenlord.UPSpatcher.AvaloniaApp;

internal static class Program
{
    [System.STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}


