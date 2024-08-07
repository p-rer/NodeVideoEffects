using NodeVideoEffects.Control;
using NodeVideoEffects.Type;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeVideoEffects.Editor
{
    /// <summary>
    /// Interaction logic for OutputPort.xaml
    /// </summary>
    public partial class InputPort : UserControl
    {
        private IControl? control;
        private Input? _input;
        private string _id;
        private int _index;

        Editor editor;

        public InputPort(Input input, string id, int index)
        {
            InitializeComponent();
            _input = input;
            _id = id;
            _index = index;
            portName.Content = input.Name;
            control = input.Control;
            control.PropertyChanged += OnControlPropertyChanged;
            portControl.Content = control;
            Loaded += (o, args) =>
            {
                editor = FindParent<Editor>(this);
            };
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        public object? Value
        {
            get => control?.Value;
            set
            {
                if (control != null)
                    control.Value = value;
            }
        }

        public string ID => _id;
        public int Index => _index;

        private void OnControlPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _input.Value = control.Value;
        }

        public void SetConnection(string id, int index)
        {
            _input.SetConnection(_id, _index, id, index);
            portControl.Visibility = Visibility.Hidden;
        }

        public void RemoveConnection()
        {
            _input.RemoveConnection(_id, _index);
            editor.RemoveConnectorFromInputPort(_id, _index);
            portControl.Visibility = Visibility.Visible;
        }

        private void Port_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RemoveConnection();
        }
    }
}
