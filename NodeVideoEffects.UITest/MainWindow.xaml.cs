using NodeVideoEffects.Editor;
using NodeVideoEffects.Nodes.Math;
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

            var node = new AddNode();
            editor.AddChildren(new Node(node), 20, 50);

            editor.ShowDialog();
            this.Close();
        }
    }
}