using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.LogViewer
{
    /// <summary>
    /// Interaction logic for LogItem.xaml
    /// </summary>
    public sealed partial class LogItem : INotifyPropertyChanged
    {
        private string _messageText = string.Empty;
        private string _timeText = string.Empty;
        private string _infoText = string.Empty;

        public LogItem((DateTime, LogLevel, string, object?) log)
        {
            InitializeComponent();
            TimeText = $"{log.Item1.GetDateTimeFormats('d')[0]} {log.Item1.ToLocalTime():HH:mm:ss.fff}";
            switch (log.Item2)
            {
                case LogLevel.Info:
                    Border.Background = new SolidColorBrush(new Color { R = 28, G = 177, B = 200, A = 75 });
                    Border.BorderBrush = new SolidColorBrush(new Color { R = 28, G = 177, B = 200, A = 255 });
                    InfoText = "[Info]";
                    break;
                case LogLevel.Warn:
                    Border.Background = new SolidColorBrush(new Color { R = 200, G = 178, B = 28, A = 75 });
                    Border.BorderBrush = new SolidColorBrush(new Color { R = 200, G = 178, B = 28, A = 255 });
                    InfoText = "[Warn]";
                    break;
                case LogLevel.Error:
                    Border.Background = new SolidColorBrush(new Color { R = 200, G = 28, B = 28, A = 75 });
                    Border.BorderBrush = new SolidColorBrush(new Color { R = 200, G = 28, B = 28, A = 255 });
                    InfoText = "[Error]";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(log), log.Item2, null);
            }
            MessageText = log.Item3;
            PropertyTree.RootObject = log.Item4;
            DataContext = this;
        }

        public string TimeText
        {
            get => _timeText;
            set => Set(ref _timeText, value);
        }

        public string InfoText
        {
            get => _infoText;
            set => Set(ref _infoText, value);
        }

        public string MessageText
        {
            get => _messageText;
            set => Set(ref _messageText, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
