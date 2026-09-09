using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Node.Updater;

internal sealed class UpdateNotificationWindow : Window
{
    private readonly CancellationTokenSource _cts = new();
    private readonly string _pluginDirectory;
    private readonly UpdateCheckResult _update;
    private bool _applyScheduled;
    private Button _cancelButton = null!;
    private bool _isBusy;
    private Button _laterButton = null!;

    private ProgressBar _progressBar = null!;
    private TextBlock _statusText = null!;
    private Button _updateButton = null!;

    public UpdateNotificationWindow(UpdateCheckResult update, string pluginDirectory)
    {
        _update = update;
        _pluginDirectory = pluginDirectory;

        Title = "ノード プラグインの更新";
        Width = 420;
        SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = true;

        if (Application.Current?.MainWindow is { } main && !ReferenceEquals(main, this))
        {
            Owner = main;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        Content = BuildContent();

        Closing += OnClosing;
    }

    private UIElement BuildContent()
    {
        var root = new StackPanel { Margin = new Thickness(16) };

        root.Children.Add(new TextBlock
        {
            Text = $"新しいバージョン {_update.LatestVersion} が利用可能です。",
            FontWeight = FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8)
        });

        if (!string.IsNullOrWhiteSpace(_update.ReleaseNotes))
            root.Children.Add(new TextBox
            {
                Text = Truncate(_update.ReleaseNotes, 800),
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 160,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 8)
            });

        if (!string.IsNullOrWhiteSpace(_update.ReleasePageUrl))
        {
            var link = new Hyperlink(new Run("リリースページを開く"));
            link.Click += (_, _) => OpenUrl(_update.ReleasePageUrl!);
            var linkText = new TextBlock { Margin = new Thickness(0, 0, 0, 8) };
            linkText.Inlines.Add(link);
            root.Children.Add(linkText);
        }

        _statusText = new TextBlock
        {
            Text = string.IsNullOrEmpty(_update.DownloadUrl)
                ? "自動更新用のファイルが見つかりませんでした。リリースページから手動でご確認ください。"
                : "",
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 8)
        };
        root.Children.Add(_statusText);

        _progressBar = new ProgressBar
        {
            IsIndeterminate = true,
            Height = 4,
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(0, 0, 0, 8)
        };
        root.Children.Add(_progressBar);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        _cancelButton = new Button
        {
            Content = "キャンセル",
            Margin = new Thickness(0, 0, 8, 0),
            Padding = new Thickness(12, 4, 12, 4),
            Visibility = Visibility.Collapsed
        };
        _cancelButton.Click += (_, _) => CancelDownload();

        _laterButton = new Button
        {
            Content = "後で",
            Margin = new Thickness(0, 0, 8, 0),
            Padding = new Thickness(12, 4, 12, 4)
        };
        _laterButton.Click += (_, _) => Postpone();

        _updateButton = new Button
        {
            Content = "今すぐ更新",
            Padding = new Thickness(12, 4, 12, 4),
            IsEnabled = !string.IsNullOrEmpty(_update.DownloadUrl)
        };
        _updateButton.Click += async (_, _) => await StartUpdateAsync().ConfigureAwait(true);

        buttonPanel.Children.Add(_cancelButton);
        buttonPanel.Children.Add(_laterButton);
        buttonPanel.Children.Add(_updateButton);
        root.Children.Add(buttonPanel);

        return root;
    }

    private async Task StartUpdateAsync()
    {
        if (_isBusy) return;
        _isBusy = true;

        _updateButton.IsEnabled = false;
        _laterButton.IsEnabled = false;
        _cancelButton.Visibility = Visibility.Visible;
        _progressBar.Visibility = Visibility.Visible;
        _statusText.Text = "ダウンロード中...";

        string stagingDir;
        try
        {
            stagingDir = await PluginUpdateChecker
                .DownloadAndStageUpdateAsync(_update, _cts.Token)
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            _statusText.Text = "キャンセルしました。";
            ResetToIdle();
            _isBusy = false;
            return;
        }
        catch (Exception)
        {
            _statusText.Text = "更新のダウンロードに失敗しました。";
            ResetToIdle();
            _isBusy = false;
            return;
        }

        _cancelButton.Visibility = Visibility.Collapsed;
        _statusText.Text = "適用を予約しています...";

        try
        {
            PluginUpdateChecker.ScheduleApplyOnNextExit(_update, stagingDir);
            _applyScheduled = true;

            _progressBar.Visibility = Visibility.Collapsed;
            _statusText.Text = "更新の準備が完了しました。次にYMM4を終了すると自動的に適用されます。";
            _laterButton.Content = "閉じる";
            _laterButton.IsEnabled = true;
        }
        catch (Exception)
        {
            _statusText.Text = "更新の適用予約に失敗しました。";
            ResetToIdle();
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void ResetToIdle()
    {
        _cancelButton.Visibility = Visibility.Collapsed;
        _progressBar.Visibility = Visibility.Collapsed;
        _updateButton.IsEnabled = !string.IsNullOrEmpty(_update.DownloadUrl);
        _laterButton.IsEnabled = true;
    }

    private void CancelDownload()
    {
        if (_applyScheduled) return;
        _cts.Cancel();
    }

    private void Postpone()
    {
        if (!_applyScheduled)
            PluginUpdateChecker.PostponeCheck(_pluginDirectory);
        Close();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_isBusy && !_applyScheduled)
            _cts.Cancel();

        if (!_applyScheduled)
            PluginUpdateChecker.PostponeCheck(_pluginDirectory);
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // ignore
        }
    }

    private static string Truncate(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
}