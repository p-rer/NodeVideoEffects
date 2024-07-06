using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
            //InitializeComponent();
            var editor = new NodeEditor();
            editor.Show();

            var rect = new Rectangle()
            {
                Width = 200,
                Height = 200,
                Fill = new SolidColorBrush(Colors.Red)
            };
            editor.AddChildren(rect, 20, 50);
        }
    }
}