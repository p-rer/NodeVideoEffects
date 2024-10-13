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
    public partial class NodeEditor : Window
    {
        public List<NodeInfo> Nodes
        {
            get => EditSpace.Nodes;
            set => EditSpace.Nodes = value;
        }

        public string ItemID
        {
            get => EditSpace.ItemID;
            set => EditSpace.ItemID = value;
        }

        public static readonly RoutedCommand AllSelectCommand = new RoutedCommand();
        public static readonly RoutedCommand RemoveCommand = new RoutedCommand();

        string tag;
        string commit;
        public NodeEditor()
        {
            InitializeComponent();

            tag = FileLoad("git_tag.txt");
            commit = FileLoad("git_id.txt");

            Task.Run(() =>
            {
                Explorer.Dispatcher.Invoke(() =>
                {
                    Explorer.Content = new NodeExplorer();
                });
            });

            CommandBindings.Add(new CommandBinding(
                AllSelectCommand,
                (s, e) => EditSpace.AllSelect()));

            InputBindings.Add(new KeyBinding(
                AllSelectCommand,
                new KeyGesture(Key.A, ModifierKeys.Control)));

            CommandBindings.Add(new CommandBinding(
                RemoveCommand,
                (s, e) => EditSpace.RemoveChildren()));

            InputBindings.Add(new KeyBinding(
                RemoveCommand,
                new KeyGesture(Key.Delete)));

            string FileLoad(string file_name)
            {
                var asm = Assembly.GetExecutingAssembly();
                var resName = asm.GetManifestResourceNames().FirstOrDefault(a => a.EndsWith(file_name));
                if (resName == null)
                    return string.Empty;
                using (var st = asm.GetManifestResourceStream(resName))
                {
                    if (st == null) return string.Empty;
                    var reader = new StreamReader(st);
                    return reader.ReadToEnd().Trim('\r', '\n');
                }
            }
        }

        public void AddChildren(Node obj, double x, double y)
        {
            EditSpace.AddChildren(obj, x, y);
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            new About(tag, commit) { Owner = this }.ShowDialog();
        }

        internal event PropertyChangedEventHandler NodesUpdated;

        private void EditSpace_NodesUpdated(object sender, PropertyChangedEventArgs e)
        {
            NodesUpdated?.Invoke(this, new PropertyChangedEventArgs(Title));
        }
    }
}