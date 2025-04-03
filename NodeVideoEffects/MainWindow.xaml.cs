using NodeVideoEffects.Editor;
using NodeVideoEffects.Core;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects;

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

        _tag = ResourceLoader.FileLoad("git_tag.txt");
        _commit = ResourceLoader.FileLoad("git_id.txt");

        Task.Run(() => { Explorer.Dispatcher.Invoke(() => { Explorer.Content = new NodeExplorer(); }); });

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
    }

    private void ShowAbout(object sender, RoutedEventArgs e)
    {
        new About(_tag, _commit) { Owner = this }.ShowDialog();
    }

    public event PropertyChangedEventHandler NodesUpdated = delegate { };

    internal void ClearEvents()
    {
        NodesUpdated = delegate { };
    }

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