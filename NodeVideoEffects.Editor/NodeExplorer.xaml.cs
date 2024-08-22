using NodeVideoEffects.Type;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for NodeExplorer.xaml
    /// </summary>
    public partial class NodeExplorer : UserControl
    {
        public ObservableCollection<NodesTree> Root { get; set; } = new ObservableCollection<NodesTree>();
        public NodeExplorer()
        {
            InitializeComponent();

            System.Type baseType = typeof(INode);

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                IEnumerable<System.Type> types = assembly.GetTypes()
                                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType));

                foreach (System.Type type in types)
                {
                    AddTypeToExplorerRoot(type);
                }
            }

            void AddTypeToExplorerRoot(System.Type type)
            {
                string[] namespaces = type.Namespace?.Split('.') ?? Array.Empty<string>();
                NodesTree? currentNode = null;

                foreach (string ns in namespaces)
                {
                    NodesTree? node = currentNode?.Children?.FirstOrDefault(n => n.Text == ns)
                               ?? Root.FirstOrDefault(n => n.Text == ns);

                    if (node == null)
                    {
                        node = new NodesTree { Text = ns };
                        if (currentNode == null)
                        {
                            Root.Add(node);
                        }
                        else
                        {
                            currentNode.Add(node);
                        }
                    }

                    currentNode = node;
                }

                NodesTree typeNode = new NodesTree { Text = type.Name, Type = type };
                currentNode?.Add(typeNode);
            }

            DataContext = this;
        }
    }

    public class NodesTree : INotifyPropertyChanged
    {
        private bool _IsExpanded = true;
        private string _Text = "";
        private System.Type? _Type;
        private NodesTree? _Parent;
        private ObservableCollection<NodesTree>? _Children;

        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set { _IsExpanded = value; OnPropertyChanged("IsExpanded"); }
        }

        public string Text
        {
            get { return _Text; }
            set { _Text = value; OnPropertyChanged("Text"); }
        }

        public System.Type? Type
        {
            get { return _Type; }
            set { _Type = value; OnPropertyChanged("Text"); }
        }

        public NodesTree? Parent
        {
            get { return _Parent; }
            set { _Parent = value; OnPropertyChanged("Parent"); }
        }

        public ObservableCollection<NodesTree>? Children
        {
            get { return _Children; }
            set { _Children = value; OnPropertyChanged("Children"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public void Add(NodesTree child)
        {
            if (null == Children) Children = new ObservableCollection<NodesTree>();
            child.Parent = this;
            Children.Add(child);
        }
    }
}
