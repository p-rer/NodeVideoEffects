using System.ComponentModel;
using System.Diagnostics; // (INotifyPropertyChanged interface)
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace NodeVideoEffects.Updater;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class UpdaterWindow
{
    private readonly UpdateViewModel _vm = new();
    public UpdaterWindow()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    public void Update(string url, string lockFile = "NodeVideoEffects.dll")
    {
        var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            lockFile);
        _ = _vm.PerformUpdateAsync(url, fileName);
    }

    private void UpdaterWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        DragMove();
    }
}

public class UpdateViewModel : INotifyPropertyChanged
{
    private double _downloadProgress;
    private bool _isIndeterminate;

    /// <summary>
    /// Represents the download progress in percentage (0 to 100).
    /// </summary>
    public double DownloadProgress
    {
        get => _downloadProgress;
        private set
        {
            if (!(Math.Abs(_downloadProgress - value) > 0.1)) return;
            _downloadProgress = value;
            OnPropertyChanged(nameof(DownloadProgress));
        }
    }
    
    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        private set
        {
            if (_isIndeterminate == value) return;
            _isIndeterminate = value;
            OnPropertyChanged(nameof(IsIndeterminate));
            OnPropertyChanged(nameof(ProgressVisible));
        }
    }

    public Visibility ProgressVisible => _isIndeterminate ? Visibility.Hidden : Visibility.Visible;

    private string _status = string.Empty;

    /// <summary>
    /// Represents the current status message.
    /// </summary>
    public string Status
    {
        get => _status;
        private set
        {
            if (_status == value) return;
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private FileStream? _stream;

    /// <summary>
    /// Performs the update process by downloading the update package,
    /// waiting for the target file to be unlocked, extracting the package,
    /// and executing a batch file to overwrite target files.
    /// </summary>
    /// <param name="updateUrl">The URL of the update ZIP package.</param>
    /// <param name="targetFilePath">
    /// The full path of the file to wait for (e.g. the main executable)
    /// that needs to be replaced.
    /// </param>
    public async Task PerformUpdateAsync(string updateUrl, string targetFilePath)
    {
        try
        {
            Status = "Starting update process...";
            var tempFolder = Path.GetTempPath();
            var zipFilePath = Path.Combine(tempFolder, "ymm4_node_video_effects_update.zip");
            var extractFolder = Path.Combine(tempFolder, "update_extracted");

            // Step 1: Download the update package
            IsIndeterminate = false;
            await DownloadFileAsync(updateUrl, zipFilePath);

            // Step 2: Wait until the target file is unlocked
            Status = "Waiting for target file to be unlocked...";
            IsIndeterminate = true;
            await WaitForFileUnlockAsync(targetFilePath);

            // Step 3: Extract the downloaded ZIP package
            Status = "Extracting update package...";
            IsIndeterminate = false;
            await ExtractZipFile(zipFilePath, extractFolder);

            // Step 4: Execute the batch file to perform the file replacement (self-update)
            Status = "Executing update batch file...";
            IsIndeterminate = true;
            ExecuteUpdateBatch(extractFolder, Path.GetDirectoryName(targetFilePath));

            Status = "Update initiated. The updater will now exit.";
            // Exit current process to allow batch file to replace locked files (self-update)
        }
        finally
        {
            _stream?.Close();
        }

        Environment.Exit(0);
    }

    /// <summary>
    /// Asynchronously downloads a file from the specified URL and saves it to the destination path.
    /// </summary>
    private async Task DownloadFileAsync(string url, string destinationFilePath)
    {
        Status = "Downloading update package...";
        try
        {
            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                await using (var contentStream = await response.Content.ReadAsStreamAsync())
                await using (var fileStream =
                             new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var totalRead = 0L;
                    var buffer = new byte[8192];
                    int read;
                    while ((read = await contentStream.ReadAsync(buffer)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, read));
                        totalRead += read;
                        if (totalBytes > 0)
                        {
                            DownloadProgress = (double)totalRead / totalBytes * 100;
                        }
                        else
                        {
                            DownloadProgress = 0; // Unknown total size
                        }
                    }
                }
            }

            DownloadProgress = 100;
            Status = "Download completed.";
        }
        catch (Exception ex)
        {
            Status = "Error during download: " + ex.Message;
            throw;
        }
    }

    /// <summary>
    /// Waits asynchronously until the specified file is no longer locked.
    /// </summary>
    private async Task WaitForFileUnlockAsync(string filePath)
    {
        while (IsFileLocked(filePath, out _stream))
        {
            await Task.Delay(1000); // Wait 1 second before retrying
        }
    }

    /// <summary>
    /// Checks if a file is locked by trying to open it exclusively.
    /// </summary>
    private static bool IsFileLocked(string filePath, out FileStream? fileStream)
    {
        FileStream? stream = null;
        try
        {
            stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite);
        }
        catch (IOException)
        {
            return true;
        }
        finally
        {
            fileStream = stream;
        }

        return false;
    }

    /// <summary>
    /// Extracts the ZIP package to the specified extraction folder.
    /// </summary>
    private async Task ExtractZipFile(string zipPath, string extractFolder)
    {
        try
        {
            var progress = new Progress<double>(completed => {
                DownloadProgress = completed * 100;
            });
            // Delete the extraction folder if it exists
            if (Directory.Exists(extractFolder))
            {
                Directory.Delete(extractFolder, true);
            }

            Directory.CreateDirectory(extractFolder);
            await ZipExtractor.ExtractToDirectoryAsync(zipPath, extractFolder, progress);
        }
        catch
        {
            //ignore
        }
        finally{
            try
            {
                File.Delete(zipPath);
            }
            catch
            {
                //ignore
            }
        }
    }

    /// <summary>
    /// Writes an embedded batch file to perform the file replacement (including self-update)
    /// and executes it.
    /// </summary>
    /// <param name="sourceFolder">The folder containing the extracted update files.</param>
    /// <param name="targetFolder">The target folder where files will be overwritten.</param>
    private static void ExecuteUpdateBatch(string sourceFolder, string? targetFolder)
    {
        // The batch file performs the following operations:
        //   1. Waits 2 seconds for the process to terminate.
        //   2. Uses xcopy to copy files from the extracted folder to the target folder recursively and overwrite.
        //   3. Deletes the temporary extracted folder.

        var batchContent = $"""
                            @echo off
                            timeout /t 2 /nobreak > NUL
                            xcopy "{sourceFolder}" "{targetFolder}" /E /Y /I
                            rd /S /Q "{sourceFolder}"
                            """;

        // Write the batch file to a temporary location
        var batFilePath = Path.Combine(Path.GetTempPath(), "update.bat");
        File.WriteAllText(batFilePath, batchContent);

        // Execute the batch file without creating a window
        var psi = new ProcessStartInfo
        {
            FileName = batFilePath,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(psi);
    }
}

public static class ZipExtractor
{
    public static async Task ExtractToDirectoryAsync(
        string zipFilePath, 
        string destinationPath, 
        IProgress<double>? progress = null, 
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(destinationPath);

        await using var zipStream = new FileStream(zipFilePath, FileMode.Open);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        var totalEntries = archive.Entries.Count;
        var processedEntries = 0;

        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fullPath = Path.Combine(destinationPath, entry.FullName);
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\'))
            {
                processedEntries++;
                continue;
            }

            await using (var entryStream = entry.Open())
            await using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await entryStream.CopyToAsync(fileStream, 81920, cancellationToken);
            }

            processedEntries++;
            progress?.Report((double)processedEntries / totalEntries);
        }
    }
}

