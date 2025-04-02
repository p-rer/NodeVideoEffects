using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.LogViewer;

/// <summary>
/// Interaction logic for LogViewer.xaml
/// </summary>
public partial class LogViewer
{
    private static LogViewer? _openedWindow;
    private ImmutableList<(DateTime, LogLevel, string, object?)> _logs = [];
    private double _currentTargetOffset;
    private bool _isBottom = true;

    public LogViewer()
    {
        if (_openedWindow != null)
        {
            _openedWindow.Activate();
            return;
        }

        Owner = Application.Current.MainWindow;
        InitializeComponent();
        _openedWindow = this;
        Logger.LogUpdated += (_, _) => Update();
        Loaded += (_, _) => Update();
        Closed += (_, _) => _openedWindow = null;
    }

    public static bool CreateWindow(out LogViewer window)
    {
        if (_openedWindow != null)
        {
            _openedWindow.Activate();
            window = _openedWindow;
            return false;
        }
        window = new LogViewer();
        return true;
    }

    private void Update()
    {
        var currentLogs = Logger.Read();
        foreach (var log in currentLogs.Where(log => !_logs.Contains(log)))
        {
            Dispatcher.Invoke(() => Viewer.Children.Add(new LogItem(log)));
        }
        if(_isBottom)
            Dispatcher.Invoke(ScrollViewer.ScrollToBottom);
        _logs = currentLogs;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        _currentTargetOffset = Math.Max(0, Math.Min(
            scrollViewer.VerticalOffset - e.Delta, scrollViewer.ScrollableHeight));

        scrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
        AnimateScroll(scrollViewer, _currentTargetOffset);

        e.Handled = true;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;
        const double scrollStep = 30;
        _currentTargetOffset = e.Key switch
        {
            Key.Up => Math.Max(0, scrollViewer.VerticalOffset - scrollStep),
            Key.Down => Math.Min(scrollViewer.ScrollableHeight, scrollViewer.VerticalOffset + scrollStep),
            Key.PageUp => Math.Max(0, scrollViewer.VerticalOffset - scrollViewer.ActualHeight),
            Key.PageDown => Math.Min(scrollViewer.ScrollableHeight, scrollViewer.VerticalOffset + scrollViewer.ActualHeight),
            _ => _currentTargetOffset
        };

        e.Handled = true;

        if (!e.Handled) return;
        scrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
        AnimateScroll(scrollViewer, _currentTargetOffset);
    }

    private static void AnimateScroll(ScrollViewer scrollViewer, double targetOffset)
    {
        var animation = new DoubleAnimation
        {
            From = scrollViewer.VerticalOffset,
            To = targetOffset,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase()
        };

        animation.Completed += (_, _) => scrollViewer.ScrollToVerticalOffset(targetOffset);

        scrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, animation);
    }

    private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        _isBottom = sender is ScrollViewer scrollViewer && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight;
    }
}

/// <summary>
/// Helper class to add attached properties for ScrollViewer animations.
/// </summary>
public static class ScrollViewerBehavior
{
    public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.RegisterAttached("VerticalOffset",
            typeof(double),
            typeof(ScrollViewerBehavior),
            new PropertyMetadata(0.0, OnVerticalOffsetChanged));

    public static double GetVerticalOffset(DependencyObject obj)
    {
        return (double)obj.GetValue(VerticalOffsetProperty);
    }

    public static void SetVerticalOffset(DependencyObject obj, double value)
    {
        obj.SetValue(VerticalOffsetProperty, value);
    }

    private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }
    }
}