namespace NodeVideoEffects.Updater;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly string[] _args = Environment.GetCommandLineArgs();
    
    public App()
    {
        if (_args.Length <= 1) return;
        var updater = new UpdaterWindow();
        updater.Show();
        if (_args.Length == 2)
            updater.Update(_args[1]);
        else
            updater.Update(_args[1], _args[2]);
    }
}