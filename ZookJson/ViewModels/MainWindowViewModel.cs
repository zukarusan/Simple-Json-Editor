using Avalonia.Controls;
using AvaloniaEdit.Document;
using ReactiveUI;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using ZookJson.Views;
using Newtonsoft.Json;

namespace ZookJson.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> OpenCommand { get; }
        public ReactiveCommand<Unit, Unit> CompactCommand { get; }
        public ReactiveCommand<Unit, Unit> PrettifyCommand { get; }
        public ReactiveCommand<Unit, Unit> CopyCommand { get; }
        public ReactiveCommand<Unit, Unit> CsvCommand { get; }
        OpenFileDialog OpenFileDialog { get; }

        readonly MainWindow? _view;
        public string Greeting => "Welcome to Avalonia!";
        public MainWindowViewModel() : this(null) {}
        public MainWindowViewModel(Window? view) 
        {
            _view = (MainWindow)view;
            OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.AllowMultiple = false;
            OpenFileDialog.Title = "Choose a JSON file";
            OpenFileDialog.Filters?.Add(new() { Name = "JSON", Extensions = { "json" } });
            OpenCommand = ReactiveCommand.CreateFromTask(Open);
            CompactCommand = ReactiveCommand.CreateFromTask(Minify);
            PrettifyCommand = ReactiveCommand.CreateFromTask(Prettify);
        }
        public async Task Open()
        {
            var result = await OpenFileDialog.ShowAsync(_view);

            if (result != null)
            {
                string path = result[0];
                using FileStream fs = File.OpenRead(path) ;
                using var sr = new StreamReader(fs, Encoding.UTF8);
                _view.Editor.Editor.Document = new TextDocument(sr.ReadToEnd());
            }
        }
        public async Task Minify()
        {
            _view.Editor.Editor.Document.Text = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(_view.Editor.Editor.Document.Text), Formatting.None);
        }
        public async Task Prettify()
        {
            _view.Editor.Editor.Document.Text = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(_view.Editor.Editor.Document.Text), Formatting.Indented);
        }
    }
}
