// ReSharper disable RedundantUsingDirective

using System.Reflection;
using System.Text.Json.Serialization;
using System.Windows;
using AvalonDock;
using AvalonDock.Layout;
using Newtonsoft.Json;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Commons;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace NodeVideoEffects;

/// <summary>
/// Interaction logic for ManageNodes.xaml
/// </summary>
public partial class OpenNodeEditorButton : IPropertyEditorControl2
{
    public OpenNodeEditorButton()
    {
        InitializeComponent();
    }

    public ItemProperty[]? ItemProperties { get; set; }
    public event EventHandler? BeginEdit;
    public event EventHandler? EndEdit;

    public void SetEditorInfo(IEditorInfo info)
    {
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (ItemProperties is null) throw new InvalidOperationException(Text_UI.ItemPropertiesNotSet);
        if (((NodeVideoEffectsPlugin)ItemProperties[0].Item).Editor != null) return;

        BeginEdit?.Invoke(this, EventArgs.Empty);

        var pluginItem = (NodeVideoEffectsPlugin)ItemProperties[0].Item;

        var parentWindow = Window.GetWindow(this)!;
        var dockingManager = parentWindow.GetType().GetField("docker",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (dockingManager == null) return;
        if (dockingManager.GetValue(parentWindow) is not DockingManager dockingManagerInstance) return;

        var editor = ((NodeVideoEffectsPlugin)ItemProperties[0].Item).Editor = new NodeEditor(dockingManagerInstance)
        {
            Nodes = pluginItem.Nodes,
            ItemId = pluginItem.Id
        };

        var floatingService = new FloatingWindowService(dockingManagerInstance);
        var anchorable = floatingService.CreateFloating(
            editor,
            Text_UI.Editor,
            800,
            450
        );

        editor.CommandBindings.AddRange(parentWindow.CommandBindings);

        editor.NodesUpdated += (_, _) =>
        {
            if (editor == null) return;

            BeginEdit?.Invoke(this, EventArgs.Empty);
            pluginItem.EditorNodes = editor.Nodes;
            EndEdit?.Invoke(this, EventArgs.Empty);
        };

        anchorable.Hiding += (_, _) =>
        {
            if (ItemProperties == null) return;
            editor.ClearEvents();
            pluginItem.Editor = null;
            editor = ((NodeVideoEffectsPlugin)ItemProperties[0].Item).Editor = null;
            var parent = anchorable.Parent;
            parent?.RemoveChild(anchorable);
        };

        editor.NeedToClose += (_, _) =>
        {
            anchorable.Close();
            editor = ((NodeVideoEffectsPlugin)ItemProperties[0].Item).Editor = null;
        };

        EndEdit?.Invoke(this, EventArgs.Empty);
    }
}

public class FloatingWindowService
{
    private readonly LayoutRoot _layout;
    private readonly DockingManager _manager;

    public FloatingWindowService(DockingManager manager)
    {
        _manager = manager;
        _layout = manager.Layout;
    }

    public LayoutAnchorable CreateFloating(UIElement content, string title, double minWidth, double minHeight)
    {
        var anchorable = new LayoutAnchorable
        {
            Title = title,
            Content = content
        };

        var pane = new LayoutAnchorablePane(anchorable)
        {
            DockMinHeight = minHeight,
            DockMinWidth = minWidth
        };

        var floatingWindow = new LayoutAnchorableFloatingWindow
        {
            RootPanel = new LayoutAnchorablePaneGroup(pane),
            Parent = _layout
        };

        _layout.FloatingWindows.Add(floatingWindow);

        _manager.UpdateLayout();
        anchorable.Float();
        anchorable.IsActive = true;

        return anchorable;
    }
}