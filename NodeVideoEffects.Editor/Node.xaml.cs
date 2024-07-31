using NodeVideoEffects.Type;
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

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Node : UserControl
    {
        public Node(INode node)
        {
            InitializeComponent();
            nodeName.Content = node.Name;
            foreach (Output output in node.Outputs)
            {
                inputsPanel.Children.Add(new OutputPort(output));
            }
            foreach (Input input in node.Inputs)
            {
                inputsPanel.Children.Add(new InputPort(input));
            }
        }
    }
}
