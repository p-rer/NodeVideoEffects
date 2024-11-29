using NodeVideoEffects.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NodeVideoEffects
{
    /// <summary>
    /// Interaction logic for LogItem.xaml
    /// </summary>
    public partial class LogItem : UserControl
    {
        public LogItem((DateTime, LogLevel, string) log)
        {
            InitializeComponent();
            time.Content = log.Item1.ToString("dd, MMM HH:mm:ss.ff");
            switch (log.Item2)
            {
                case LogLevel.Info:
                    border.Background = new SolidColorBrush(new() { R=33, G=49, B=77, A=255 });
                    border.BorderBrush = new SolidColorBrush(new() { R=20, G=51, B=143, A=255 });
                    loglevel.Content = "[Info]";
                    break;
                case LogLevel.Warn:
                    border.Background = new SolidColorBrush(new() { R = 79, G = 52, B = 20, A = 255 });
                    border.BorderBrush = new SolidColorBrush(new() { R = 143, G = 85, B = 20, A = 255 });
                    loglevel.Content = "[Warn]";
                    break;
                case LogLevel.Error:
                    border.Background = new SolidColorBrush(new() { R = 69, G = 6, B = 17, A = 255 });
                    border.BorderBrush = new SolidColorBrush(new() { R = 120, G = 17, B = 36, A = 255 });
                    loglevel.Content = "[Error]";
                    break;
            }
            message.Content = log.Item3;
        }
    }
}
