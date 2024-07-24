using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NodeVideoEffects.UITest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.Hide();

            var editor = new NodeEditor();

            var rect = new Rectangle()
            {
                Width = 200,
                Height = 200,
                Fill = new SolidColorBrush(Colors.Red)
            };
            editor.AddChildren(rect, 20, 50);

            editor.ShowDialog();
            this.Close();
        }
    }
}