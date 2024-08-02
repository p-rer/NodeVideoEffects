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
    /// Interaction logic for OutputPort.xaml
    /// </summary>
    public partial class OutputPort : UserControl
    {
        string _id;
        int _index;
        public OutputPort(Output output, string id, int index)
        {
            InitializeComponent();
            _id = id;
            _index = index;
            portName.Content = output.Name;
            ToolTip = new();
            ToolTipOpening += OutputPort_ToolTipOpening;
        }

        private void OutputPort_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            Task<object> @object = NodesManager.GetOutputValue(_id, _index);
            @object.Wait();
            ToolTip = @object.Result;
        }
    }
}
