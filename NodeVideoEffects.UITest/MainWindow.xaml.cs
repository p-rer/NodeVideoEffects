using NodeVideoEffects.Nodes.Math;
using NodeVideoEffects.Type;
using System.Runtime.InteropServices;
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
            [DllImport("Kernel32")]
            static extern void AllocConsole();

            var id = Guid.NewGuid().ToString("N");

            var editor = new NodeEditor()
            {
                ItemID = id
            };
            var nodes = new List<NodeInfo>();

            var node1 = new AddNode();
            var node2 = new PowNode();

            node1.Id = id + "-" + Guid.NewGuid().ToString("N");
            node2.Id = id + "-" + Guid.NewGuid().ToString("N");

            NodesManager.AddNode(node1.Id, node1);
            NodesManager.AddNode(node2.Id, node2);

            node2.SetInputConnection(1, new(node1.Id, 0));

            nodes.Add(new(node1.Id, node1.GetType(), [], 100, 100, [new(), new()]));
            nodes.Add(new(node2.Id, node2.GetType(), [], 500, 100, [new(), new(node1.Id, 0)]));

            editor.Nodes = nodes;
            AllocConsole();

            editor.ShowDialog();
            this.Close();
        }
    }
}