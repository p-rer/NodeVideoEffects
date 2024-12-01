using NodeVideoEffects.Utility;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace NodeVideoEffects
{
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : Window
    {
        static LogViewer? OpenedWindow = null;
        private ImmutableList<(DateTime, LogLevel, string)> logs = ImmutableList<(DateTime, LogLevel, string)>.Empty;
        private double _currentTargetOffset = 0;

        public LogViewer()
        {
            if (OpenedWindow != null)
            {
                OpenedWindow.Activate();
                return;
            }
            InitializeComponent();
            OpenedWindow = this;
            Logger.LogUpdated += (s, e) => Update();
            Loaded += (s, e) => Update();
            Closed += (s, e) => OpenedWindow = null;
        }

        public static bool CreateWindow(out LogViewer window)
        {
            if (OpenedWindow != null)
            {
                OpenedWindow.Activate();
                window = OpenedWindow;
                return false;
            }
            window = new();
            return true;
        }

        private void Update()
        {
            var currentLogs = Logger.Read();
            foreach (var log in currentLogs)
            {
                if (!logs.Contains(log))
                {
                    Dispatcher.Invoke(() => viewer.Children.Add(new LogItem(log)));
                }
            }
            logs = currentLogs;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                _currentTargetOffset = Math.Max(0, Math.Min(
                    _currentTargetOffset - e.Delta, scrollViewer.ScrollableHeight));

                scrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
                AnimateScroll(scrollViewer, _currentTargetOffset);

                e.Handled = true;
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                double scrollStep = 30;
                if (e.Key == Key.Up)
                {
                    _currentTargetOffset = Math.Max(0, _currentTargetOffset - scrollStep);
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    _currentTargetOffset = Math.Min(scrollViewer.ScrollableHeight, _currentTargetOffset + scrollStep);
                    e.Handled = true;
                }
                else if (e.Key == Key.PageUp)
                {
                    _currentTargetOffset = Math.Max(0, _currentTargetOffset - scrollViewer.ActualHeight);
                    e.Handled = true;
                }
                else if (e.Key == Key.PageDown)
                {
                    _currentTargetOffset = Math.Min(scrollViewer.ScrollableHeight, _currentTargetOffset + scrollViewer.ActualHeight);
                    e.Handled = true;
                }

                if (e.Handled)
                {
                    scrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, null);
                    AnimateScroll(scrollViewer, _currentTargetOffset);
                }
            }
        }

        private void AnimateScroll(ScrollViewer scrollViewer, double targetOffset)
        {
            var animation = new DoubleAnimation
            {
                From = scrollViewer.VerticalOffset,
                To = targetOffset,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase()
            };

            animation.Completed += (s, e) => scrollViewer.ScrollToVerticalOffset(targetOffset);

            scrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, animation);
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
}
