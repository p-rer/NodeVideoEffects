using System.Windows;

namespace NodeVideoEffects;

/// <summary>
/// Interaction logic for About.xaml
/// </summary>
public partial class About
{
    public About(string tag, string commit)
    {
        InitializeComponent();
        Ver.Content = $"{tag} (Git {commit})";
    }

    private void Close(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}