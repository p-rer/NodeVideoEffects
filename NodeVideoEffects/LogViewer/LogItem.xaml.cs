using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.LogViewer
{
    public sealed partial class LogItem : INotifyPropertyChanged
    {
        public LogItem()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LogTimeProperty =
            DependencyProperty.Register(
                nameof(LogTime),
                typeof(DateTime),
                typeof(LogItem),
                new PropertyMetadata(DateTime.Now, OnLogTimeChanged));

        public DateTime LogTime
        {
            get => (DateTime)GetValue(LogTimeProperty);
            set => SetValue(LogTimeProperty, value);
        }

        private static void OnLogTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (LogItem)d;
            
            ctl.TimeText = $"{ctl.LogTime.GetDateTimeFormats('d')[0]} {ctl.LogTime.ToLocalTime():HH:mm:ss.fff}";
        }

        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register(
                nameof(Level),
                typeof(LogLevel),
                typeof(LogItem),
                new PropertyMetadata(OnLevelChanged));

        public LogLevel Level
        {
            get => (LogLevel)GetValue(LevelProperty);
            set => SetValue(LevelProperty, value);
        }

        private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (LogItem)d;
            switch (ctl.Level)
            {
                case LogLevel.Debug:
                    ctl.Border.Background = Brushes.Transparent;
                    ctl.Border.BorderBrush = Brushes.Transparent;
                    break;
                case LogLevel.Info:
                    ctl.Border.Background = new SolidColorBrush(Color.FromArgb(75, 28, 177, 200));
                    ctl.Border.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 28, 177, 200));
                    break;
                case LogLevel.Warn:
                    ctl.Border.Background = new SolidColorBrush(Color.FromArgb(75, 200, 178, 28));
                    ctl.Border.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 200, 178, 28));
                    break;
                case LogLevel.Error:
                    ctl.Border.Background = new SolidColorBrush(Color.FromArgb(75, 200, 28, 28));
                    ctl.Border.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 200, 28, 28));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ctl.InfoText = $"[{ctl.Level}]";
        }

        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register(
                nameof(MessageText),
                typeof(string),
                typeof(LogItem),
                new PropertyMetadata(string.Empty));

        public string MessageText
        {
            get => (string)GetValue(MessageTextProperty);
            set => SetValue(MessageTextProperty, value);
        }

        public static readonly DependencyProperty RootObjectProperty =
            DependencyProperty.Register(
                nameof(RootObject),
                typeof(object),
                typeof(LogItem),
                new PropertyMetadata(null, OnRootObjectChanged));

        public object? RootObject
        {
            get => GetValue(RootObjectProperty);
            set => SetValue(RootObjectProperty, value);
        }

        private static void OnRootObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctl = (LogItem)d;
            ctl.PropertyTree.RootObject = e.NewValue;
        }
        
        private string _timeText = string.Empty;
        public string TimeText
        {
            get => _timeText;
            private set => Set(ref _timeText, value);
        }

        private string _infoText = string.Empty;
        public string InfoText
        {
            get => _infoText;
            private set => Set(ref _infoText, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
