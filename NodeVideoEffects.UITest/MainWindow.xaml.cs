using NodeVideoEffects.Editor;
using NodeVideoEffects.Nodes.Math;
using System.Windows;

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

            var node1 = new AddNode();
            editor.AddChildren(new Node(node1), 20, 50);

            var node2 = new PowNode();
            editor.AddChildren(new Node(node2), 80, 90);

            editor.ShowDialog();
            this.Close();
        }
    }
}