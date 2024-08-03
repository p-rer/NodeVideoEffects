﻿using NodeVideoEffects.Control;
using NodeVideoEffects.Type;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class InputPort : UserControl
    {
        private IControl? control;
        private Input? _input;
        private string _id;
        private int _index;

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
            portControl.Visibility = Visibility.Visible;
        }

        private void Port_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RemoveConnection();
        }
    }
}
