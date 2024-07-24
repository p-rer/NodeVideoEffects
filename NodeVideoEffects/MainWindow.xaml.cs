using System.IO;
using System.Reflection;
using System.Windows;

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
                if (st == null) commitid = string.Empty;
                var reader = new StreamReader(st);
                commitid = reader.ReadToEnd().Trim('\r', '\n');
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