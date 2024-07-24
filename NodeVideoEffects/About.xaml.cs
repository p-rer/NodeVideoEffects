using System.Windows;

namespace NodeVideoEffects
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About(string git)
        {
            InitializeComponent();
            this.git.Content = $"Git Commit ID: {git}";
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
