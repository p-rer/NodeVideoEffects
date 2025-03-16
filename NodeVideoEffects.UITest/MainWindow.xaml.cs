using NodeVideoEffects.Nodes.Math;
using NodeVideoEffects.Type;
using System.Runtime.InteropServices;
using NodeVideoEffects.Utility;

namespace NodeVideoEffects.UITest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            Hide();

            var id = Guid.NewGuid().ToString("N");

            var editor = new NodeEditor()
            {
                ItemId = id
            };
            var nodes = new List<NodeInfo>();

            var node1 = new AddNode();
            var node2 = new PowNode();

            node1.Id = id + "-" + Guid.NewGuid().ToString("N");
            node2.Id = id + "-" + Guid.NewGuid().ToString("N");

            NodesManager.AddNode(node1.Id, node1);
            NodesManager.AddNode(node2.Id, node2);

            node2.SetInputConnection(1, new PortInfo(node1.Id, 0));

            nodes.Add(new NodeInfo(node1.Id, node1.GetType(), [], 100, 100, [new PortInfo(), new PortInfo()]));
            nodes.Add(new NodeInfo(node2.Id, node2.GetType(), [], 500, 100,
                [new PortInfo(), new PortInfo(node1.Id, 0)]));

            editor.Nodes = nodes;
            AllocConsole();
            editor.NodesUpdated += (_, _) => Logger.Write(LogLevel.Info, "Nodes updated", editor.Nodes);
            _ = new UpdaterService();

            editor.ShowDialog();
            Close();
            return;

            [DllImport("Kernel32")]
            static extern void AllocConsole();
        }
    }
}