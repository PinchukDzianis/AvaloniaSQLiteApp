using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaSQLiteApp.Views;
using OfficeOpenXml;
using AvaloniaSQLiteApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Data;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaSQLiteApp
{
    public class App : Application
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static IServiceProvider Services { get; private set; }

        private IPasswordService _passwordService;
        private static string dbPath;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public override void Initialize()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            string dbFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvaloniaSQLiteApp");

            Directory.CreateDirectory(dbFolder);

            dbPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AvaloniaSQLiteApp"), "database.db");

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<AppDbContext>(options =>
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }); ;
            serviceCollection.AddSingleton<IPasswordService, PasswordService>();
            serviceCollection.AddSingleton<LoginWindow>();
            serviceCollection.AddSingleton<MainWindow>();

            Services = serviceCollection.BuildServiceProvider();

            _passwordService = Services.GetRequiredService<IPasswordService>();

            EnsureDatabase();
            CreateDefaultUser();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = Services.GetRequiredService<LoginWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void EnsureDatabase()
        {
            if (!File.Exists(dbPath))
            {
                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();

                string createUsersTable = @"CREATE TABLE Users (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Login TEXT UNIQUE NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    PasswordSalt TEXT NOT NULL
                );";

                string createModesTable = @"CREATE TABLE Modes (
                    ID INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    MaxBottleNumber INTEGER,
                    MaxUsedTips INTEGER
                );";

                string createStepsTable = @"CREATE TABLE Steps (
                    ID INTEGER PRIMARY KEY,
                    ModeId INTEGER,
                    Timer INTEGER,
                    Destination TEXT,
                    Speed INTEGER,
                    Type TEXT,
                    Volume INTEGER,
                    FOREIGN KEY(ModeId) REFERENCES Modes(ID)
                );";

                using var command = connection.CreateCommand();
                command.CommandText = createUsersTable;
                command.ExecuteNonQuery();

                command.CommandText = createModesTable;
                command.ExecuteNonQuery();

                command.CommandText = createStepsTable;
                command.ExecuteNonQuery();
            }
        }

        private void CreateDefaultUser()
        {
            string defaultLogin = "test";
            string defaultPassword = "Test123";

            if (!AuthenticateUser(defaultLogin, defaultPassword))
            {
                RegisterUser(defaultLogin, defaultPassword);
            }
        }

        public bool RegisterUser(string login, string password)
        {
            if (password.Length < 6 || !HasLetterAndDigit(password))
                return false;

            var (Salt, Hash) = _passwordService.HashPassword(password);
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Users (Login, PasswordHash, PasswordSalt) VALUES (@login, @passwordHash, @passwordSalt);";
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@passwordHash", Hash);
            command.Parameters.AddWithValue("@passwordSalt", Salt);

            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AuthenticateUser(string login, string password)
        {
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT PasswordHash, PasswordSalt FROM Users WHERE Login = @login";
            command.Parameters.AddWithValue("@login", login);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                string storedHash = reader.GetString(0);
                string storedSalt = reader.GetString(1);
                return _passwordService.VerifyPassword(password, storedHash, storedSalt);
            }
            return false;
        }     

        private static bool HasLetterAndDigit(string password)
        {
            bool hasLetter = false, hasDigit = false;
            foreach (char c in password)
            {
                if (char.IsLetter(c))
                    hasLetter = true;
                if (char.IsDigit(c))
                    hasDigit = true;
                if (hasLetter && hasDigit)
                    return true;
            }
            return false;
        }
    }
}
