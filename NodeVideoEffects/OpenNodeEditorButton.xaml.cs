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
#if DEBUG
        Logger.Write(LogLevel.Debug, "Button_Click event handler started.", new { Sender = sender, EventArgs = e });
#endif

        if (ItemProperties is null)
        {
#if DEBUG
            Logger.Write(LogLevel.Error, "ItemProperties is null. Throwing exception.");
#endif
            throw new InvalidOperationException("ItemProperties is not set.");
        }

#if DEBUG
        Logger.Write(LogLevel.Debug, "Checking if the NodeVideoEffectsPlugin Window is already set.",
            (NodeVideoEffectsPlugin)ItemProperties[0].Item);
#endif
        if (((NodeVideoEffectsPlugin)ItemProperties[0].Item).Window != null)
        {
#if DEBUG
            Logger.Write(LogLevel.Debug, "Window already exists. Exiting Button_Click handler.",
                (NodeVideoEffectsPlugin)ItemProperties[0].Item);
#endif
            return;
        }

#if DEBUG
        Logger.Write(LogLevel.Debug, "Invoking BeginEdit event.");
#endif
        BeginEdit?.Invoke(this, EventArgs.Empty);

        var pluginItem = (NodeVideoEffectsPlugin)ItemProperties[0].Item;
#if DEBUG
        Logger.Write(LogLevel.Debug, "Creating new NodeEditor window.", pluginItem);
#endif
        var window = pluginItem.Window = new NodeEditor
        {
            Owner = Window.GetWindow(this),
            Nodes = pluginItem.Nodes,
            ItemId = pluginItem.Id
        };

#if DEBUG
        Logger.Write(LogLevel.Debug, "Adding parent window's CommandBindings to the new window if available.",
            window);
#endif
        var parentWindow = Window.GetWindow(this);
        if (parentWindow != null)
        {
            window.CommandBindings.AddRange(parentWindow.CommandBindings);
#if DEBUG
            Logger.Write(LogLevel.Debug, "Parent CommandBindings added.", parentWindow.CommandBindings);
#endif
        }

#if DEBUG
        Logger.Write(LogLevel.Debug, "Showing the NodeEditor window.", window);
#endif
        window.Show();

#if DEBUG
        Logger.Write(LogLevel.Debug, "Subscribing to NodesUpdated event.", window);
#endif
        window.NodesUpdated += (_, _) =>
        {
            if (window == null) return;
#if DEBUG
            Logger.Write(LogLevel.Debug, "NodesUpdated event triggered.", window.Nodes);
#endif
            BeginEdit?.Invoke(this, EventArgs.Empty);
            pluginItem.EditorNodes = window.Nodes;
#if DEBUG
            Logger.Write(LogLevel.Debug, "EditorNodes updated with new nodes from window.");
#endif
            EndEdit?.Invoke(this, EventArgs.Empty);
#if DEBUG
            Logger.Write(LogLevel.Debug, "EndEdit event invoked after NodesUpdated.");
#endif
        };

#if DEBUG
        Logger.Write(LogLevel.Debug, "Subscribing to Closed event of the window.", window);
#endif
        window.Closed += (_, _) =>
        {
#if DEBUG
            Logger.Write(LogLevel.Debug, "Window Closed event triggered.");
#endif
            if (ItemProperties == null) return;
#if DEBUG
            Logger.Write(LogLevel.Debug, "Clearing Window property of the plugin item.", pluginItem);
#endif
            window.ClearEvents();
            window = pluginItem.Window = null;
        };

#if DEBUG
        Logger.Write(LogLevel.Debug, "Invoking EndEdit event after setting up window.");
#endif
        EndEdit?.Invoke(this, EventArgs.Empty);

#if DEBUG
        Logger.Write(LogLevel.Debug, "Button_Click event handler completed.");
#endif
    }

    public void SetEditorInfo(IEditorInfo info)
    {
    }
}