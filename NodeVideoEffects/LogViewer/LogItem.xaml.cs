using System.Windows.Media;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.LogViewer
{
    /// <summary>
    /// Interaction logic for LogItem.xaml
    /// </summary>
    public partial class LogItem
    {
        public LogItem((DateTime, LogLevel, string) log)
        {
            InitializeComponent();
            Time.Content = log.Item1.ToString("dd, MMM HH:mm:ss.ff");
            switch (log.Item2)
            {
                case LogLevel.Info:
                    Border.Background = new SolidColorBrush(new() { R = 33, G = 49, B = 77, A = 255 });
                    Border.BorderBrush = new SolidColorBrush(new() { R = 20, G = 51, B = 143, A = 255 });
                    Loglevel.Content = "[Info]";
                    break;
                case LogLevel.Warn:
                    Border.Background = new SolidColorBrush(new() { R = 79, G = 52, B = 20, A = 255 });
                    Border.BorderBrush = new SolidColorBrush(new() { R = 143, G = 85, B = 20, A = 255 });
                    Loglevel.Content = "[Warn]";
                    break;
                case LogLevel.Error:
                    Border.Background = new SolidColorBrush(new() { R = 69, G = 6, B = 17, A = 255 });
                    Border.BorderBrush = new SolidColorBrush(new() { R = 120, G = 17, B = 36, A = 255 });
                    Loglevel.Content = "[Error]";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Message.Content = log.Item3;
        }
    }
}
