using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NodeVideoEffects.Control;

public class StackPanelPort : StackPanel, IControl
{
    private List<object?> _value = [];
    public event PropertyChangedEventHandler? PropertyChanged;

    public object? Value
    {
        get => _value;
        set => _value = value as List<object?> ?? [];
    }

    public StackPanelPort(List<(FrameworkElement, string)> children)
    {
        Orientation = Orientation.Vertical;
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if (child.Item1 is not IControl control) continue;
            var index = i;
            control.PropertyChanged += (s, e) => ControlOnPropertyChanged(s, index, e);
            child.Item1.VerticalAlignment = VerticalAlignment.Center;
            Children.Add(
                new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = child.Item2,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(10, 0, 10, 0)
                        }, child.Item1
                    },
                    Orientation = Orientation.Horizontal
                }
            );
            _value.Add(control.Value);
        }
    }

    private void ControlOnPropertyChanged(object? sender, int index, PropertyChangedEventArgs args)
    {
        if (sender is not IControl control) return;
        _value[index] = control.Value;
        PropertyChanged?.Invoke(sender, args);
    }
}