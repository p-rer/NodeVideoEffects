using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NodeVideoEffects;

public enum DockLocation
{
    TopLeft,
    Top,
    TopRight,

    Left,
    Center,
    Right,

    BottomLeft,
    Bottom,
    BottomRight
}

public class DockablePanel : ContentControl
{
    static DockablePanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DockablePanel),
            new FrameworkPropertyMetadata(typeof(DockablePanel)));
        FocusableProperty.OverrideMetadata(typeof(DockablePanel),
            new FrameworkPropertyMetadata(true));
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        Focus();
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(DockablePanel),
            new PropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DockLocationProperty =
        DependencyProperty.Register(
            nameof(DockLocation),
            typeof(DockLocation),
            typeof(DockablePanel),
            new PropertyMetadata(DockLocation.Center));

    public DockLocation DockLocation
    {
        get => (DockLocation)GetValue(DockLocationProperty);
        set => SetValue(DockLocationProperty, value);
    }

    public static readonly DependencyProperty PriorityProperty =
        DependencyProperty.Register(
            nameof(Priority),
            typeof(int),
            typeof(DockablePanel),
            new PropertyMetadata(0));

    public int Priority
    {
        get => (int)GetValue(PriorityProperty);
        set => SetValue(PriorityProperty, value);
    }
}