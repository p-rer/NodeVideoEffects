// ReSharper disable RedundantUsingDirective
using System.Windows;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Commons;

namespace NodeVideoEffects;

/// <summary>
/// Interaction logic for ManageNodes.xaml
/// </summary>
public partial class OpenNodeEditorButton : IPropertyEditorControl2
{
    public event EventHandler? BeginEdit;
    public event EventHandler? EndEdit;

    public ItemProperty[]? ItemProperties { get; set; }

    public OpenNodeEditorButton()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Logger.Write(LogLevel.Debug, "Button_Click event handler started.", new { Sender = sender, EventArgs = e });

        if (ItemProperties is null)
        {
            Logger.Write(LogLevel.Error, "ItemProperties is null. Throwing exception.");
            throw new InvalidOperationException("ItemProperties is not set.");
        }

        Logger.Write(LogLevel.Debug, "Checking if the NodeVideoEffectsPlugin Window is already set.",
            (NodeVideoEffectsPlugin)ItemProperties[0].Item);
        if (((NodeVideoEffectsPlugin)ItemProperties[0].Item).Window != null)
        {
            Logger.Write(LogLevel.Debug, "Window already exists. Exiting Button_Click handler.",
                (NodeVideoEffectsPlugin)ItemProperties[0].Item);
            return;
        }

        Logger.Write(LogLevel.Debug, "Invoking BeginEdit event.");
        BeginEdit?.Invoke(this, EventArgs.Empty);

        var pluginItem = (NodeVideoEffectsPlugin)ItemProperties[0].Item;
        Logger.Write(LogLevel.Debug, "Creating new NodeEditor window.", pluginItem);
        var window = pluginItem.Window = new NodeEditor
        {
            Owner = Window.GetWindow(this),
            Nodes = pluginItem.Nodes,
            ItemId = pluginItem.Id
        };

        Logger.Write(LogLevel.Debug, "Adding parent window's CommandBindings to the new window if available.",
            window);
        var parentWindow = Window.GetWindow(this);
        if (parentWindow != null)
        {
            window.CommandBindings.AddRange(parentWindow.CommandBindings);
            Logger.Write(LogLevel.Debug, "Parent CommandBindings added.", parentWindow.CommandBindings);
        }

        Logger.Write(LogLevel.Debug, "Showing the NodeEditor window.", window);
        window.Show();

        Logger.Write(LogLevel.Debug, "Subscribing to NodesUpdated event.", window);
        window.NodesUpdated += (_, _) =>
        {
            if (window == null) return;
            Logger.Write(LogLevel.Debug, "NodesUpdated event triggered.", window.Nodes);
            BeginEdit?.Invoke(this, EventArgs.Empty);
            pluginItem.EditorNodes = window.Nodes;
            Logger.Write(LogLevel.Debug, "EditorNodes updated with new nodes from window.");
            EndEdit?.Invoke(this, EventArgs.Empty);
            Logger.Write(LogLevel.Debug, "EndEdit event invoked after NodesUpdated.");
        };

        Logger.Write(LogLevel.Debug, "Subscribing to Closed event of the window.", window);
        window.Closed += (_, _) =>
        {
            Logger.Write(LogLevel.Debug, "Window Closed event triggered.");
            if (ItemProperties == null) return;
            Logger.Write(LogLevel.Debug, "Clearing Window property of the plugin item.", pluginItem);
            window.ClearEvents();
            window = pluginItem.Window = null;
        };

        Logger.Write(LogLevel.Debug, "Invoking EndEdit event after setting up window.");
        EndEdit?.Invoke(this, EventArgs.Empty);

        Logger.Write(LogLevel.Debug, "Button_Click event handler completed.");
    }

    public void SetEditorInfo(IEditorInfo info)
    {
    }
}