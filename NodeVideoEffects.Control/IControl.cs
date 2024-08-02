using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NodeVideoEffects.Control
{
    public interface IControl
    {
        public object? Value { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
