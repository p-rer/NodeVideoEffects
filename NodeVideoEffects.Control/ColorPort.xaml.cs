using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeVideoEffects.Control;

public partial class ColorPort : IControl
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private bool _suppressRgbChannelCallback;
    private bool _suppressSelectedColorCallback;
    public object? Value { get; set; }
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPort),
            new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public Color SelectedColor {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly DependencyProperty RedProperty =
        DependencyProperty.Register(nameof(Red), typeof(byte), typeof(ColorPort),
            new FrameworkPropertyMetadata((byte)255, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRgbChannelChanged));

    public static readonly DependencyProperty GreenProperty =
        DependencyProperty.Register(nameof(Green), typeof(byte), typeof(ColorPort),
            new FrameworkPropertyMetadata((byte)255, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRgbChannelChanged));

    public static readonly DependencyProperty BlueProperty =
        DependencyProperty.Register(nameof(Blue), typeof(byte), typeof(ColorPort),
            new FrameworkPropertyMetadata((byte)255, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRgbChannelChanged));

    public static readonly DependencyProperty AlphaProperty =
        DependencyProperty.Register(nameof(Alpha), typeof(byte), typeof(ColorPort),
            new FrameworkPropertyMetadata((byte)255, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRgbChannelChanged));

    public byte Red {
        get => (byte)GetValue(RedProperty);
        set => SetValue(RedProperty, value);
    }
    public byte Green {
        get => (byte)GetValue(GreenProperty);
        set => SetValue(GreenProperty, value);
    }
    public byte Blue {
        get => (byte)GetValue(BlueProperty);
        set => SetValue(BlueProperty, value);
    }
    public byte Alpha {
        get => (byte)GetValue(AlphaProperty);
        set => SetValue(AlphaProperty, value);
    }

    public ColorPort(Color color) {
        InitializeComponent();
        Value = color;
        DataContext = this;
    }

    public ColorPort() : this(Colors.White) { }

    private static void OnRgbChannelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var cp = (ColorPort)d;
        if (cp._suppressRgbChannelCallback)
            return;
        cp.UpdateColorFromRgbChannels();
    }

    private void UpdateColorFromRgbChannels() {
        var newColor = Color.FromArgb(Alpha, Red, Green, Blue);
        _suppressSelectedColorCallback = true;
        SelectedColor = newColor;
        _suppressSelectedColorCallback = false;
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var cp = (ColorPort)d;
        if (cp._suppressSelectedColorCallback)
            return;
        cp.UpdateRgbFromSelectedColor();
    }

    private void UpdateRgbFromSelectedColor() {
        _suppressRgbChannelCallback = true;
        Alpha = SelectedColor.A;
        Red = SelectedColor.R;
        Green = SelectedColor.G;
        Blue = SelectedColor.B;
        _suppressRgbChannelCallback = false;
    }

    private void btnColor_Click(object sender, RoutedEventArgs e)
    {
        Popup.IsOpen = true;
    }

    private void Popup_OnClosed(object? sender, EventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }

    private void UIElement_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}

public class ColorToHexConverter : IValueConverter {
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is Color color) {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        return "#FFFFFFFF";
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string s) return DependencyProperty.UnsetValue;
        s = s.Trim();
        if (s.StartsWith('#'))
            s = s[1..];

        try
        {
            switch (s.Length)
            {
                case 8:
                {
                    var a = byte.Parse(s[..2], NumberStyles.HexNumber);
                    var r = byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
                    var g = byte.Parse(s.Substring(4, 2), NumberStyles.HexNumber);
                    var b = byte.Parse(s.Substring(6, 2), NumberStyles.HexNumber);
                    return Color.FromArgb(a, r, g, b);
                }
                case 6:
                {
                    var r = byte.Parse(s[..2], NumberStyles.HexNumber);
                    var g = byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
                    var b = byte.Parse(s.Substring(4, 2), NumberStyles.HexNumber);
                    return Color.FromArgb(255, r, g, b);
                }
            }
        }
        catch
        {
            return DependencyProperty.UnsetValue;
        }
        return DependencyProperty.UnsetValue;
    }
}