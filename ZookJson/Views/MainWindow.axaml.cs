using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace ZookJson.Views
{
    using Pair = KeyValuePair<int, Control>;
    public partial class MainWindow : Window
    {
        private readonly TextEditor _textEditor;
        private Installation _textMateInstallation;
        private RegistryOptions _registryOptions;
        public MainWindow()
        {
            InitializeComponent();
            _textEditor = this.FindControl<EditorControl>("Editor").Editor;
            _textEditor.Background = Brushes.Transparent;
            _textEditor.ShowLineNumbers = true;
            _textEditor.TextArea.Background = this.Background;
            _textEditor.Options.ShowBoxForControlCharacters = true;
            _textEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy(_textEditor.Options);
            _textEditor.TextArea.RightClickMovesCaret = true;
            _textEditor.TextArea.TextView.ElementGenerators.Add(new ElementGenerator());

            _registryOptions = new RegistryOptions(ThemeName.DarkPlus);

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            string content = "";
            if (assets != null)
            {
                var stream = assets.Open(new Uri("avares://ZookJson/Assets/welcome.json"));
                using var sr = new StreamReader(stream, Encoding.UTF8);
                content = sr.ReadToEnd();
            }
            _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);
            _textEditor.Document = new TextDocument(
            content + Environment.NewLine);
            _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".json").Id));
        }


        class ElementGenerator : VisualLineElementGenerator, IComparer<Pair>
        {
            public List<Pair> controls = new List<Pair>();

            /// <summary>
            /// Gets the first interested offset using binary search
            /// </summary>
            /// <returns>The first interested offset.</returns>
            /// <param name="startOffset">Start offset.</param>
            public override int GetFirstInterestedOffset(int startOffset)
            {
                int pos = controls.BinarySearch(new Pair(startOffset, null), this);
                if (pos < 0)
                    pos = ~pos;
                if (pos < controls.Count)
                    return controls[pos].Key;
                else
                    return -1;
            }

            public override VisualLineElement ConstructElement(int offset)
            {
                int pos = controls.BinarySearch(new Pair(offset, null), this);
                if (pos >= 0)
                    return new InlineObjectElement(0, controls[pos].Value);
                else
                    return null;
            }

            int IComparer<Pair>.Compare(Pair x, Pair y)
            {
                return x.Key.CompareTo(y.Key);
            }
        }
    }
}
