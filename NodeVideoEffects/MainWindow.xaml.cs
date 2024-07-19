using System.IO;
using System.Reflection;
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
using System.Xml.Linq;

namespace NodeVideoEffects
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NodeEditor : Window
    {
        string commitid;
        public NodeEditor()
        {
            InitializeComponent();
            var asm = Assembly.GetExecutingAssembly();
            var resName = asm.GetManifestResourceNames().FirstOrDefault(a => a.EndsWith("git_id.txt"));
            if (resName == null)
                commitid = string.Empty;
            using (var st = asm.GetManifestResourceStream(resName))
            {
                if (st == null) commitid =  string.Empty;
                var reader = new StreamReader(st);
                commitid =  reader.ReadToEnd().Trim('\r', '\n');
            }
        }

        public void AddChildren(Object obj, double x, double y)
        {
            EditSpace.AddChildren(obj, x, y);
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            new About(commitid).ShowDialog();
        }
    }
}