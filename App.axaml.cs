using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using StockTrack.Avalonia.Views;
using StockTrack.Models;

namespace StockTrack.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.Args?.Contains("--capture-screens") == true)
            {
                RunScreenCapture(desktop);
                return;
            }

            desktop.MainWindow = new LoginWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async void RunScreenCapture(IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectScreenshotDir = Path.GetFullPath(Path.Combine(baseDir, "../../../screenshots"));
            string artifactDir = "/Users/mac/.gemini/antigravity-cli/brain/68f9f3f5-d424-4e5c-acb7-575365b29da5";

            Directory.CreateDirectory(projectScreenshotDir);
            Directory.CreateDirectory(artifactDir);
            desktop.ShutdownMode = global::Avalonia.Controls.ShutdownMode.OnExplicitShutdown;

            Console.WriteLine($"Capturing screenshots to: {projectScreenshotDir}");

            // 1. Capture Login Window
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            await Task.Delay(1000);
            SaveDualCapture(loginWindow, "01_Login_Window.png", 520, 460, projectScreenshotDir, artifactDir);
            loginWindow.Close();

            await Task.Delay(500);

            // 2. Capture Main Window Tabs
            var adminUser = new User
            {
                UserID = 1,
                Username = "admin",
                FullName = "System Administrator",
                Role = "Admin"
            };
            var mainWindow = new MainWindow(adminUser);
            desktop.MainWindow = mainWindow;
            mainWindow.Show();
            await Task.Delay(1500); // Allow MySQL queries & UI binding

            // Tab 0: Dashboard
            mainWindow.MainTabs.SelectedIndex = 0;
            await Task.Delay(600);
            SaveDualCapture(mainWindow, "02_Dashboard_Tab.png", 1280, 800, projectScreenshotDir, artifactDir);

            // Tab 1: Products
            mainWindow.MainTabs.SelectedIndex = 1;
            await Task.Delay(600);
            SaveDualCapture(mainWindow, "03_Products_Catalog_Tab.png", 1280, 800, projectScreenshotDir, artifactDir);

            // Tab 2: Stock Movements
            mainWindow.MainTabs.SelectedIndex = 2;
            await Task.Delay(600);
            SaveDualCapture(mainWindow, "04_Stock_Movements_Tab.png", 1280, 800, projectScreenshotDir, artifactDir);

            // Tab 3: Reports (populate report first)
            mainWindow.MainTabs.SelectedIndex = 3;
            mainWindow.BtnRunReport_Click(null, null);
            await Task.Delay(600);
            SaveDualCapture(mainWindow, "05_Reports_Valuation_Tab.png", 1280, 800, projectScreenshotDir, artifactDir);

            // Tab 4: Vendors & Categories
            mainWindow.MainTabs.SelectedIndex = 4;
            await Task.Delay(600);
            SaveDualCapture(mainWindow, "06_Vendors_Categories_Tab.png", 1280, 800, projectScreenshotDir, artifactDir);

            // Tab 5: User Accounts (Admin)
            mainWindow.MainTabs.SelectedIndex = 5;
            await Task.Delay(600);
            SaveDualCapture(mainWindow, "07_User_Accounts_Tab.png", 1280, 800, projectScreenshotDir, artifactDir);

            Console.WriteLine("ALL_SCREENSHOTS_CAPTURED_SUCCESSFULLY");
            mainWindow.Close();
            desktop.Shutdown(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Screenshot capture failed: {ex}");
            desktop.Shutdown(1);
        }
    }

    private void SaveDualCapture(Visual visual, string filename, int width, int height, string dir1, string dir2)
    {
        string path1 = Path.Combine(dir1, filename);
        ScreenshotHelper.CaptureVisual(visual, path1, width, height);

        if (Directory.Exists(dir2))
        {
            string path2 = Path.Combine(dir2, filename);
            File.Copy(path1, path2, true);
        }
    }
}
