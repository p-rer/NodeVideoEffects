// ReSharper disable RedundantUsingDirective

using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Newtonsoft.Json;
using NodeVideoEffects.Utility;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Settings;
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
        var registeredFloatingWindows = new HashSet<LayoutAnchorableFloatingWindowControl>();
        List<CommandBinding>? savedParentCommandBindings = null;

        // キーと実行内容の組を定義
        var keyActions = new[]
        {
            new { Key = Key.A, Modifiers = ModifierKeys.Control, Action = (Action)(() => editor.AllSelect()) },
            new { Key = Key.Delete, Modifiers = ModifierKeys.None, Action = (Action)(() => editor.RemoveChildren()) }
        };

        parentWindow.AddHandler(
            PreviewKeyDownEvent,
            new KeyEventHandler((_, keyEventArgs) =>
            {
                // ドッキング中でanchorableがアクティブな場合のみ処理
                if (anchorable.IsFloating || !anchorable.IsActive) return;

                SwitchKeyboardAction(keyEventArgs);
            }),
            true);

        // anchorableのIsActiveChangedイベントで無効化・復元を管理
        anchorable.IsActiveChanged += (_, _) =>
        {
            if (anchorable is { IsActive: true, IsFloating: false })
            {
                // アクティブになったら対象のCommandBindingsだけを無効化
                if (savedParentCommandBindings != null) return;
                savedParentCommandBindings = [];

                foreach (var keyAction in keyActions)
                {
                    var dict = KeyCommandCollector.Collect();
                    var command = dict.Where(item =>
                            item.Key.Key == keyAction.Key && item.Key.Modifiers == keyAction.Modifiers)
                        .Select(item => item.Value)
                        .FirstOrDefault();
                    if (command == null)
                        continue;

                    var matchingCommandBindings = parentWindow.CommandBindings
                        .OfType<CommandBinding>()
                        .Where(binding => binding.Command == command)
                        .ToList();

                    if (matchingCommandBindings.Count != 0)
                        savedParentCommandBindings.AddRange(matchingCommandBindings);
                }

                // 保存したCommandBindingsを削除
                foreach (var binding in savedParentCommandBindings) parentWindow.CommandBindings.Remove(binding);
            }
            else
            {
                // アクティブでなくなったら復元
                if (savedParentCommandBindings == null) return;
                foreach (var binding in savedParentCommandBindings) parentWindow.CommandBindings.Add(binding);

                savedParentCommandBindings = null;
            }
        };

        // フローティングに変わった時も復元
        anchorable.PropertyChanged += (_, propertyChangedEventArgs) =>
        {
            if (propertyChangedEventArgs.PropertyName == nameof(LayoutAnchorable.IsFloating) && anchorable.IsFloating)
                if (savedParentCommandBindings != null)
                {
                    foreach (var binding in savedParentCommandBindings) parentWindow.CommandBindings.Add(binding);

                    savedParentCommandBindings = null;
                }
        };

        dockingManagerInstance.LayoutUpdated += CheckAndRegisterFloatingWindow;
        CheckAndRegisterFloatingWindow(null, EventArgs.Empty);

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
        return;

        void SwitchKeyboardAction(KeyEventArgs keyEventArgs)
        {
            var matchingAction = keyActions.FirstOrDefault(ka =>
                ka.Key == keyEventArgs.Key && ka.Modifiers == Keyboard.Modifiers);

            if (matchingAction != null)
            {
                matchingAction.Action();
                keyEventArgs.Handled = true;
            }
        }

        // DockingManagerのLayoutUpdatedイベントで監視
        void CheckAndRegisterFloatingWindow(object? _, EventArgs __)
        {
            var layoutFloatingWindow = anchorable.FindParent<LayoutAnchorableFloatingWindow>();
            if (layoutFloatingWindow == null) return;
            var floatingWindowControl = dockingManagerInstance.FloatingWindows
                .OfType<LayoutAnchorableFloatingWindowControl>()
                .FirstOrDefault(w => Equals(w.Model, layoutFloatingWindow));

            if (floatingWindowControl != null) RegisterFloatingWindowEvents(floatingWindowControl);
        }

        // フローティングウィンドウのイベントを登録
        void RegisterFloatingWindowEvents(LayoutAnchorableFloatingWindowControl floatingWindowControl)
        {
            if (!registeredFloatingWindows.Add(floatingWindowControl))
                return;

            floatingWindowControl.Activated += (_, _) =>
            {
                // アクティブになったら対象のCommandBindingsだけを無効化
                if (savedParentCommandBindings != null) return;
                savedParentCommandBindings = [];

                foreach (var keyAction in keyActions)
                {
                    var dict = KeyCommandCollector.Collect();
                    var command = dict.Where(item =>
                            item.Key.Key == keyAction.Key && item.Key.Modifiers == keyAction.Modifiers)
                        .Select(item => item.Value)
                        .FirstOrDefault();
                    if (command == null)
                        continue;

                    var matchingCommandBindings = parentWindow.CommandBindings
                        .OfType<CommandBinding>()
                        .Where(binding => binding.Command == command)
                        .ToList();

                    if (matchingCommandBindings.Count != 0)
                        savedParentCommandBindings.AddRange(matchingCommandBindings);
                }

                // 保存したCommandBindingsを削除
                foreach (var binding in savedParentCommandBindings) parentWindow.CommandBindings.Remove(binding);
            };

            floatingWindowControl.Deactivated += (_, _) =>
            {
                if (savedParentCommandBindings == null) return;
                foreach (var binding in savedParentCommandBindings) parentWindow.CommandBindings.Add(binding);

                savedParentCommandBindings = null;
            };

            floatingWindowControl.AddHandler(
                PreviewKeyDownEvent,
                new KeyEventHandler((_, keyEventArgs) => SwitchKeyboardAction(keyEventArgs)),
                true);

            floatingWindowControl.Closed += (_, _) => { registeredFloatingWindows.Remove(floatingWindowControl); };
        }
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