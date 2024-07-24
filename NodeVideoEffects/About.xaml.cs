using System.Windows;

namespace NodeVideoEffects
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About(string tag, string commit)
        {
            InitializeComponent();
            this.ver.Content = $"{tag} (Git {commit})";
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
