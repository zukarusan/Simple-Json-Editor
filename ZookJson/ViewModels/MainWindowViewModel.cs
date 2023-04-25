using Avalonia.Controls;
using AvaloniaEdit.Document;
using ReactiveUI;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using ZookJson.Views;
using Newtonsoft.Json;
using Avalonia;
using MessageBox.Avalonia;
using System;
using Avalonia.Platform;
using Avalonia.Threading;
using Newtonsoft.Json.Linq;

namespace ZookJson.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> OpenCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveAsCommand { get; }
        public ReactiveCommand<Unit, Unit> CompactCommand { get; }
        public ReactiveCommand<Unit, Unit> PrettifyCommand { get; }
        public ReactiveCommand<Unit, Unit> CopyCommand { get; }
        public ReactiveCommand<Unit, Unit> CsvCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
        public ReactiveCommand<Unit, Unit> DirectCsvCommand { get; }
        private string? _currentFile = null;
        private string? _currentFolder = null;
        public string? CurrentFile { get { return _currentFile; } }
        OpenFileDialog OpenFileDialog { get; }
        SaveFileDialog SaveJSONDialog { get; }
        SaveFileDialog SaveCSVDialog { get; }
        private ProgressWindow CsvProgress { get; set;  }
        public string Greeting => "Welcome to Avalonia!";
        public MainWindowViewModel(Window? view) : base(view)
        {
            if (view != null && view is not MainWindow)
            {
                throw new ArgumentException("View must be MainWindow");
            }
            
            OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.AllowMultiple = false;
            OpenFileDialog.Title = "Choose a JSON file";
            OpenFileDialog.Filters?.Add(new() { Name = "JSON", Extensions = { "json" } });

            SaveJSONDialog= new SaveFileDialog();
            SaveJSONDialog.Title = "Choose filename and path to save";
            SaveJSONDialog.Filters?.Add(new() { Name="JSON", Extensions = { "json" } });
            SaveJSONDialog.DefaultExtension = "json";

            SaveCSVDialog = new SaveFileDialog();
            SaveCSVDialog.Title = "Choose filename and path to export";
            SaveCSVDialog.Filters?.Add(new() { Name = "CSV", Extensions = { "csv" } });
            SaveCSVDialog.DefaultExtension = "csv";

            OpenCommand = ReactiveCommand.CreateFromTask(Open);
            SaveCommand = ReactiveCommand.CreateFromTask(Save);
            SaveAsCommand = ReactiveCommand.CreateFromTask(SaveAs);
            CompactCommand = ReactiveCommand.CreateFromTask(Minify);
            PrettifyCommand = ReactiveCommand.CreateFromTask(Prettify);
            CopyCommand = ReactiveCommand.CreateFromTask(Copy);
            ExitCommand = ReactiveCommand.CreateFromTask(Exit);
            CsvCommand = ReactiveCommand.CreateFromTask(Csv);
            CsvCommand.ThrownExceptions.Subscribe((error) => {
                MessageBoxManager.GetMessageBoxStandardWindow("Cannot export CSV, Unexpected Error occured",
                    $"Message: {error.Message}{Environment.NewLine}").Show();
            });
            DirectCsvCommand = ReactiveCommand.CreateFromTask(DirectCsv);
            RecreateProgressWindow();
        }
        private void RecreateProgressWindow()
        {
            CsvProgress = new ProgressWindow(_view);
            CsvProgress.Title = "Generating Csv";
            CsvProgress.ExtendClientAreaToDecorationsHint = true;
            CsvProgress.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            CsvProgress.ExtendClientAreaTitleBarHeightHint = -1;
        }
        public async Task Open()
        {
            OpenFileDialog.Directory = Path.GetDirectoryName(CurrentFile);
            var result = await OpenFileDialog.ShowAsync(_view);

            if (result != null)
            {
                string path = result[0];
                using FileStream fs = File.OpenRead(path) ;
                using var sr = new StreamReader(fs, Encoding.UTF8);
                ((MainWindow)_view).Editor.Editor.Document = new TextDocument(sr.ReadToEnd());
                _currentFile = Path.GetFullPath(path);
            }
        }
        public async Task Save()
        {
            if (CurrentFile == null)
            {
                await SaveAs();
                return;
            }
            await File.WriteAllTextAsync(CurrentFile, ((MainWindow)_view).Editor.Editor.Document.Text);
        }
        public async Task SaveAs()
        {
            SaveJSONDialog.Directory = Path.GetDirectoryName(CurrentFile);
            SaveJSONDialog.InitialFileName = "MyJson.json";
            var result = await SaveJSONDialog.ShowAsync(_view);

            if (result != null)
            {
                string path = result;
                _currentFile = Path.GetFullPath(path);
                await File.WriteAllTextAsync(path, ((MainWindow)_view).Editor.Editor.Document.Text);
            }
        }
        public async Task Exit()
        {
            _view.Close();
        }
        public async Task Minify()
        {
            ((MainWindow)_view).Editor.Editor.Document.Text = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(((MainWindow)_view).Editor.Editor.Document.Text), Formatting.None);
        }
        public async Task Prettify()
        {
            ((MainWindow)_view).Editor.Editor.Document.Text = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(((MainWindow)_view).Editor.Editor.Document.Text), Formatting.Indented);
        }
        public async Task Copy()
        {
            await Application.Current.Clipboard.SetTextAsync(((MainWindow)_view).Editor.Editor.Document.Text);
        }
        public async Task DirectCsv()
        {
            OpenFileDialog.Directory = Path.GetDirectoryName(CurrentFile);
            var result = await OpenFileDialog.ShowAsync(_view);

            if (result != null)
            {
                string path = result[0];
                try
                {
                    CsvProgress.Active = true;
                }
                catch (InvalidOperationException)
                {
                    RecreateProgressWindow();
                    CsvProgress.Active = true;
                }
                string csv = Path.Combine(Path.GetDirectoryName(path), $"Exported_Json_{DateTime.Now.ToString("dd-MM-yyyy.HH.mm.ss")}.csv");
                Dispatcher.UIThread.Post(async () => await ConvertToCSV(path, csv), DispatcherPriority.Background);
            }

        }
        public async Task Csv()
        {
            SaveCSVDialog.InitialFileName = $"Exported_Json_{DateTime.Now.ToString("dd-MM-yyyy.HH.mm.ss")}.csv";
            SaveCSVDialog.Directory = _currentFolder != null ? _currentFolder : SaveCSVDialog.Directory;
            var result = await SaveCSVDialog.ShowAsync(_view);
            if (result != null)
            {
                _currentFolder = Path.GetDirectoryName(result);
                try
                {
                    CsvProgress.Active = true;
                }
                catch (InvalidOperationException)
                {
                    RecreateProgressWindow();
                    CsvProgress.Active = true;
                }
                Dispatcher.UIThread.Post(async () => await ConvertToCSV(result), DispatcherPriority.Background);
            }
        }
        private async Task ConvertToCSV(string path)
        {
            string jsonContent = ((MainWindow)_view).Editor.Editor.Document.Text;
            await Task.Run(()=> Util.JsonToCSV(jsonContent, path,
            () => {
                Dispatcher.UIThread.InvokeAsync(() => CsvProgress.Active = false, DispatcherPriority.DataBind);
            }));
        }
        private async Task ConvertToCSV(string jsonPath, string csvPath)
        {
            await Task.Run(()=>Util.FileJsonToCSV(jsonPath, csvPath,
            () => {
                Dispatcher.UIThread.InvokeAsync(() => CsvProgress.Active = false, DispatcherPriority.DataBind);
                string argument = $@"/select, ""{csvPath}""";

                System.Diagnostics.Process.Start("explorer.exe", argument);
            }));
        }
    }
}
