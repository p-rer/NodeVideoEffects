// ReSharper disable RedundantUsingDirective

using System.Text.Json.Serialization;
using System.Windows;
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
        if (((NodeVideoEffectsPlugin)ItemProperties[0].Item).Window != null) return;

        BeginEdit?.Invoke(this, EventArgs.Empty);

        var pluginItem = (NodeVideoEffectsPlugin)ItemProperties[0].Item;
        var window = pluginItem.Window = new NodeEditor
        {
            Owner = Window.GetWindow(this),
            Nodes = pluginItem.Nodes,
            ItemId = pluginItem.Id
        };

        var parentWindow = Window.GetWindow(this);
        if (parentWindow != null) window.CommandBindings.AddRange(parentWindow.CommandBindings);

        window.Show();

        window.NodesUpdated += (_, _) =>
        {
            if (window == null) return;

            BeginEdit?.Invoke(this, EventArgs.Empty);
            pluginItem.EditorNodes = window.Nodes;
            EndEdit?.Invoke(this, EventArgs.Empty);
        };

        window.Closing += (_, _) =>
        {
            if (ItemProperties == null) return;
            window.ClearEvents();
            pluginItem.Window = null;
            window = null;
        };

        EndEdit?.Invoke(this, EventArgs.Empty);
    }
}