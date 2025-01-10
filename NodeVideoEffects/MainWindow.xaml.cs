using NodeVideoEffects.Editor;
using NodeVideoEffects.Type;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace NodeVideoEffects
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NodeEditor
    {
        public List<NodeInfo> Nodes
        {
            get => EditSpace.Nodes;
            set => EditSpace.Nodes = value;
        }

        public string ItemId
        {
            get => EditSpace.ItemId;
            set => EditSpace.ItemId = value;
        }

        private static readonly RoutedCommand AllSelectCommand = new();
        private static readonly RoutedCommand RemoveCommand = new();

        private readonly string _tag;
        private readonly string _commit;

        public NodeEditor()
        {
            InitializeComponent();

            _tag = FileLoad("git_tag.txt");
            _commit = FileLoad("git_id.txt");

            Task.Run(() =>
            {
                Explorer.Dispatcher.Invoke(() =>
                {
                    Explorer.Content = new NodeExplorer();
                });
            });

            CommandBindings.Add(new CommandBinding(
                AllSelectCommand,
                (_, _) => EditSpace.AllSelect()));

            InputBindings.Add(new KeyBinding(
                AllSelectCommand,
                new KeyGesture(Key.A, ModifierKeys.Control)));

            CommandBindings.Add(new CommandBinding(
                RemoveCommand,
                (_, _) => EditSpace.RemoveChildren()));

            InputBindings.Add(new KeyBinding(
                RemoveCommand,
                new KeyGesture(Key.Delete)));
            return;

            string FileLoad(string fileName)
            {
                var asm = Assembly.GetExecutingAssembly();
                var resName = asm.GetManifestResourceNames().FirstOrDefault(a => a.EndsWith(fileName));
                if (resName == null)
                    return string.Empty;
                using var st = asm.GetManifestResourceStream(resName);
                if (st == null) return string.Empty;
                var reader = new StreamReader(st);
                return reader.ReadToEnd().Trim('\r', '\n');
            }
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            new About(_tag, _commit) { Owner = this }.ShowDialog();
        }

        internal event PropertyChangedEventHandler NodesUpdated = delegate { };

        private void EditSpace_NodesUpdated(object sender, PropertyChangedEventArgs e)
        {
            NodesUpdated.Invoke(this, new PropertyChangedEventArgs(Title));
        }

        private void OpenLogViewer(object sender, RoutedEventArgs e)
        {
            if (LogViewer.LogViewer.CreateWindow(out var viewer))
                viewer.Show();
        }
    }
}