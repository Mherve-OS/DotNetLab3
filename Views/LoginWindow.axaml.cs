using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StockTrack.Controllers;
using StockTrack.Models;

namespace StockTrack.Avalonia.Views;

public partial class LoginWindow : Window
{
    private readonly AuthController _authController;

    public LoginWindow()
    {
        InitializeComponent();

        var userRepo = new UserRepository(DatabaseConfig.ConnectionString);
        var auditRepo = new AuditRepository(DatabaseConfig.ConnectionString);
        _authController = new AuthController(userRepo, auditRepo);

        TxtUsername.KeyDown += OnInputKeyDown;
        TxtPassword.KeyDown += OnInputKeyDown;
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            BtnLogin_Click(sender, e);
        }
    }

    private void BtnLogin_Click(object? sender, RoutedEventArgs e)
    {
        string username = TxtUsername.Text?.Trim() ?? string.Empty;
        string password = TxtPassword.Text ?? string.Empty;

        PnlError.IsVisible = false;

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError("Please enter your username.");
            TxtUsername.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError("Please enter your password.");
            TxtPassword.Focus();
            return;
        }

        try
        {
            var user = _authController.Login(username, password);

            var mainWindow = new MainWindow(user);
            mainWindow.Show();

            this.Close();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            TxtPassword.Text = string.Empty;
            TxtPassword.Focus();
        }
    }

    private void ShowError(string message)
    {
        TxtError.Text = message;
        PnlError.IsVisible = true;
    }
}
