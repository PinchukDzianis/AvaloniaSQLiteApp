using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using AvaloniaSQLiteApp.Services;
using System;
using Data;
using System.Threading.Tasks;

namespace AvaloniaSQLiteApp.Views
{
    public partial class LoginWindow : Window
    {
        private readonly TextBox _loginTextBox = null!;
        private readonly TextBox _passwordBox = null!;
        private readonly TextBlock _messageTextBlock = null!;
        private readonly IPasswordService _passwordService;
        private readonly AppDbContext _dbContext;
        private readonly MainWindow _mainWindow;

        public LoginWindow() : this(null!, null!, null!) { }

        public LoginWindow(IPasswordService passwordService, AppDbContext appDbContext, MainWindow mainWindow)
        {
            InitializeComponent();

            _loginTextBox = this.FindControl<TextBox>("LoginTextBox")
                ?? throw new Exception("LoginTextBox not found in XAML.");
            _passwordBox = this.FindControl<TextBox>("PasswordTextBox")
                ?? throw new Exception("TextBoxPassword not found in XAML.");
            _messageTextBlock = this.FindControl<TextBlock>("MessageTextBlock")
                ?? throw new Exception("MessageTextBlock not found in XAML.");
            
            _passwordService = passwordService;
            _dbContext = appDbContext;
            _mainWindow = mainWindow;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OnLoginClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string login = _loginTextBox.Text ?? "";
            string password = _passwordBox.Text ?? "";

            if (await AuthenticateUserAsync(login, password))
            {
                _messageTextBlock.Text = "Login successful!";
                _mainWindow.Show();
                this.Close();
            }
            else
            {
                _messageTextBlock.Text = "Invalid login or password.";
            }
        }

        private async void OnRegisterClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string login = _loginTextBox.Text ?? "";
            string password = _passwordBox.Text ?? "";

            if (await RegisterUserAsync(login, password))
            {
                _messageTextBlock.Text = "Registration successful! You can now log in.";
            }
            else
            {
                _messageTextBlock.Text = "Registration failed. Login might already exist or password is weak.";
            }
        }

        private async Task<bool> RegisterUserAsync(string login, string password)
        {
            bool result = false;
         
            if (password.Length >= 6 && HasLetterAndDigit(password))
            {
                var (Salt, Hash) = _passwordService.HashPassword(password);

                try
                {
                    var user = new User
                    {
                        Login = login,
                        PasswordHash = Hash,
                        PasswordSalt = Salt
                    };
                 
                    _dbContext.Users.Add(user);
                 
                    await _dbContext.SaveChangesAsync();

                    result = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Auth error: {ex.Message}");
                    result = false;
                }
            }

            return result;
        }

        private async Task<bool> AuthenticateUserAsync(string login, string password)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Login == login);

                if (user != null)
                {
                    return _passwordService.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Auth error: {ex.Message}");
            }

            return false;
        }

        private static bool HasLetterAndDigit(string password)
        {
            bool hasLetter = false, hasDigit = false;
            foreach (char c in password)
            {
                if (char.IsLetter(c)) hasLetter = true;
                if (char.IsDigit(c)) hasDigit = true;
                if (hasLetter && hasDigit) return true;
            }
            return false;
        }
    }
}
