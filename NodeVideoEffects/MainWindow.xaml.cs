﻿using System.IO;
using System.Reflection;
using System.Windows;

namespace NodeVideoEffects
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NodeEditor : Window
    {
        string tag;
        string commit;
        public NodeEditor()
        {
            InitializeComponent();

            tag = FileLoad("git_tag.txt");
            commit = FileLoad("git_id.txt");

            string FileLoad(string file_name)
            {
                var asm = Assembly.GetExecutingAssembly();
                var resName = asm.GetManifestResourceNames().FirstOrDefault(a => a.EndsWith(file_name));
                if (resName == null)
                    return string.Empty;
                using (var st = asm.GetManifestResourceStream(resName))
                {
                    if (st == null) return string.Empty;
                    var reader = new StreamReader(st);
                    return reader.ReadToEnd().Trim('\r', '\n');
                }
            }
        }

        public void AddChildren(Object obj, double x, double y)
        {
            EditSpace.AddChildren(obj, x, y);
        }

        private void ShowAbout(object sender, RoutedEventArgs e)
        {
            new About(tag, commit) { Owner = this }.ShowDialog();
        }
    }
}