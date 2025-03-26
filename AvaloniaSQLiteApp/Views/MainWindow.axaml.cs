using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Microsoft.Data.Sqlite;
using OfficeOpenXml;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using AvaloniaSQLiteApp.Models;

namespace AvaloniaSQLiteApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly DataGrid _modesDataGrid = null!;
        private readonly DataGrid _stepsDataGrid = null!;

        public ObservableCollection<Mode> Modes { get; set; } = [];
        public ObservableCollection<Step> Steps { get; set; } = [];

        public MainWindow()
        {
            InitializeComponent();
            _modesDataGrid = this.FindControl<DataGrid>("ModesDataGrid")!;
            _stepsDataGrid = this.FindControl<DataGrid>("StepsDataGrid")!;

            this.DataContext = this;
            LoadData();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void LoadData()
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            var modesCommand = connection.CreateCommand();
            modesCommand.CommandText = "SELECT ID, Name, MaxBottleNumber, MaxUsedTips FROM Modes";
            using var modesReader = modesCommand.ExecuteReader();
            var modes = new ObservableCollection<Mode>();
            while (modesReader.Read())
            {
                modes.Add(new Mode
                {
                    ID = modesReader.GetInt32(0),
                    Name = modesReader.GetString(1),
                    MaxBottleNumber = modesReader.GetInt32(2),
                    MaxUsedTips = modesReader.GetInt32(3)
                });
            }

            var stepsCommand = connection.CreateCommand();
            stepsCommand.CommandText = "SELECT ID, ModeId, Timer, Destination, Speed, Type, Volume FROM Steps";
            using var stepsReader = stepsCommand.ExecuteReader();
            var steps = new ObservableCollection<Step>();
            while (stepsReader.Read())
            {
                steps.Add(new Step
                {
                    ID = stepsReader.GetInt32(0),
                    ModeId = stepsReader.GetInt32(1),
                    Timer = stepsReader.GetInt32(2),
                    Destination = stepsReader.GetString(3),
                    Speed = stepsReader.GetInt32(4),
                    Type = stepsReader.GetString(5),
                    Volume = stepsReader.GetInt32(6)
                });
            }

            Modes.Clear();
            foreach (var m in modes)
                Modes.Add(m);

            Steps.Clear();
            foreach (var s in steps)
                Steps.Add(s);

            _modesDataGrid.ItemsSource = Modes;
            _stepsDataGrid.ItemsSource = Steps;

            Console.WriteLine($"Loaded {Modes.Count} modes and {Steps.Count} steps.");
        }

        private async void OnImportExcelClick(object sender, RoutedEventArgs e)
        {
            var options = new FilePickerOpenOptions
            {
                Title = "בונטעו Excel פאיכ",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new("Excel Files") { Patterns = ["*.xlsx", "*.xls"] }
                ]
            };

            var result = await StorageProvider.OpenFilePickerAsync(options);
            if (result != null && result.Count > 0)
            {
                string filePath = result[0].Path.LocalPath;
                ImportExcelData(filePath);
            }
        }

        private void ImportExcelData(string filePath)
        {
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var modesWorksheet = package.Workbook.Worksheets["Modes"];
                    if (modesWorksheet != null)
                    {
                        for (int row = 2; row <= modesWorksheet.Dimension.End.Row; row++)
                        {
                            var modeIdText = modesWorksheet.Cells[row, 1].Text;
                            var modeName = modesWorksheet.Cells[row, 2].Text;
                            var maxBottleNumberText = modesWorksheet.Cells[row, 3].Text;
                            var maxUsedTipsText = modesWorksheet.Cells[row, 4].Text;

                            if (!string.IsNullOrWhiteSpace(modeIdText))
                            {
                                int modeId = int.Parse(modeIdText);
                                int maxBottleNumber = int.Parse(maxBottleNumberText);
                                int maxUsedTips = int.Parse(maxUsedTipsText);

                                if (!ModeExists(modeId))
                                {
                                    InsertModeIntoDatabase(modeId, modeName, maxBottleNumber, maxUsedTips);
                                }
                            }
                        }
                    }

                    var stepsWorksheet = package.Workbook.Worksheets["Steps"];
                    if (stepsWorksheet != null)
                    {
                        for (int row = 2; row <= stepsWorksheet.Dimension.End.Row; row++)
                        {
                            var idText = stepsWorksheet.Cells[row, 1].Text;
                            var modeIdText = stepsWorksheet.Cells[row, 2].Text;
                            var timerText = stepsWorksheet.Cells[row, 3].Text;
                            string destination = string.IsNullOrEmpty(stepsWorksheet.Cells[row, 4].Text) ? string.Empty : stepsWorksheet.Cells[row, 4].Text;
                            var speedText = stepsWorksheet.Cells[row, 5].Text;
                            var type = stepsWorksheet.Cells[row, 6].Text;
                            var volumeText = stepsWorksheet.Cells[row, 7].Text;

                            if (!string.IsNullOrWhiteSpace(idText))
                            {
                                int id = int.Parse(idText);
                                int modeId = int.Parse(modeIdText);
                                int timer = int.Parse(timerText);
                                int speed = int.Parse(speedText);
                                int volume = int.Parse(volumeText);

                                if (!StepExists(id))
                                {
                                    InsertStepIntoDatabase(id, modeId, timer, destination, speed, type, volume);
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("Data imported successfully.");
                LoadData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"רטבךא ןנט מבנאבמעךו פאיכא Excel: {ex.Message}");
            }
        }

        private static bool ModeExists(int modeId)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Modes WHERE ID = @ID";
            command.Parameters.AddWithValue("@ID", modeId);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        private static bool StepExists(int id)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Steps WHERE ID = @ID";
            command.Parameters.AddWithValue("@ID", id);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        private static void InsertModeIntoDatabase(int modeId, string name, int maxBottleNumber, int maxUsedTips)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Modes (ID, Name, MaxBottleNumber, MaxUsedTips) VALUES (@ID, @name, @maxBottleNumber, @maxUsedTips)";
            command.Parameters.AddWithValue("@ID", modeId);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@maxBottleNumber", maxBottleNumber);
            command.Parameters.AddWithValue("@maxUsedTips", maxUsedTips);
            command.ExecuteNonQuery();
        }

        private static void InsertStepIntoDatabase(int id, int modeId, int timer, string destination, int speed, string type, int volume)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Steps (ID, ModeId, Timer, Destination, Speed, Type, Volume) VALUES (@ID, @modeId, @timer, @destination, @speed, @type, @volume)";
            command.Parameters.AddWithValue("@ID", id);
            command.Parameters.AddWithValue("@modeId", modeId);
            command.Parameters.AddWithValue("@timer", timer);
            command.Parameters.AddWithValue("@destination", destination);
            command.Parameters.AddWithValue("@speed", speed);
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@volume", volume);
            command.ExecuteNonQuery();
        }

        private void OnEditStepClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Step row)
            {
                UpdateStepInDatabase(row);
                int index = Steps.IndexOf(row);
                if (index >= 0)
                {
                    Steps[index] = row;
                }
            }
        }

        private void OnDeleteStepClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Step row)
            {
                DeleteStepFromDatabase(row.ID);
                Steps.Remove(row);
            }
        }

        private void OnEditModeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Mode row)
            {
                UpdateModeInDatabase(row);
                int index = Modes.IndexOf(row);
                if (index >= 0)
                {
                    Modes[index] = row;
                }
            }
        }

        private void OnDeleteModeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Mode row)
            {
                DeleteStepsByModeFromDatabase(row.ID);
                var stepsToRemove = Steps.Where(x => x.ModeId == row.ID).ToList();
                foreach (var step in stepsToRemove)
                {
                    Steps.Remove(step);
                }
                DeleteModeFromDatabase(row.ID);
                Modes.Remove(row);
            }
        }

        private static void DeleteModeFromDatabase(int id)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Modes WHERE ID = @ID";
            command.Parameters.AddWithValue("@ID", id);
            command.ExecuteNonQuery();
        }

        private static void DeleteStepFromDatabase(int id)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Steps WHERE ID = @ID";
            command.Parameters.AddWithValue("@ID", id);
            command.ExecuteNonQuery();
        }

        private static void DeleteStepsByModeFromDatabase(int modeId)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Steps WHERE ModeId = @ModeId";
            command.Parameters.AddWithValue("@ModeId", modeId);
            command.ExecuteNonQuery();
        }

        private static void UpdateStepInDatabase(Step step)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Steps 
                SET ModeId = @ModeId,
                    Timer = @Timer,
                    Destination = @Destination, 
                    Speed = @Speed, 
                    Type = @Type,
                    Volume = @Volume
                WHERE ID = @ID";
            command.Parameters.AddWithValue("@ID", step.ID);
            command.Parameters.AddWithValue("@ModeId", step.ModeId);
            command.Parameters.AddWithValue("@Timer", step.Timer);
            command.Parameters.AddWithValue("@Destination", step.Destination);
            command.Parameters.AddWithValue("@Speed", step.Speed);
            command.Parameters.AddWithValue("@Type", step.Type);
            command.Parameters.AddWithValue("@Volume", step.Volume);
            command.ExecuteNonQuery();
        }

        private static void UpdateModeInDatabase(Mode mode)
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "database.db");
            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Modes 
                SET Name = @Name,
                    MaxBottleNumber = @MaxBottleNumber,
                    MaxUsedTips = @MaxUsedTips
                WHERE ID = @ID";
            command.Parameters.AddWithValue("@ID", mode.ID);
            command.Parameters.AddWithValue("@Name", mode.Name);
            command.Parameters.AddWithValue("@MaxBottleNumber", mode.MaxBottleNumber);
            command.Parameters.AddWithValue("@MaxUsedTips", mode.MaxUsedTips);
            command.ExecuteNonQuery();
        }
    }
}
