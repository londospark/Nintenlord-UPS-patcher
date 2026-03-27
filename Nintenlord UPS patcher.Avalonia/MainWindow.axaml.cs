using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Nintenlord.Hacking.Core;

namespace Nintenlord.UPSpatcher.AvaloniaApp;

public partial class MainWindow : Window
{
    private CancellationTokenSource _patchCts;

    // Stored so we can open them via OpenReadAsync() instead of relying on path strings,
    // which can be unreliable on some platforms (notably Windows paths with spaces).
    private IStorageFile _patchStorageFile;
    private IStorageFile _inspectStorageFile;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void BrowsePatchTargetFile_Click(object sender, RoutedEventArgs e) =>
        PatchTargetFileTextBox.Text = await PickFilePathAsync("Select a file", false);

    private async void BrowsePatchFile_Click(object sender, RoutedEventArgs e)
    {
        _patchStorageFile = await PickStorageFileAsync("Select a patch", true);
        PatchFileTextBox.Text = _patchStorageFile?.TryGetLocalPath() ?? _patchStorageFile?.Name ?? string.Empty;
    }

    private async void BrowseOriginalFile_Click(object sender, RoutedEventArgs e) =>
        OriginalFileTextBox.Text = await PickFilePathAsync("Select the original file", false);

    private async void BrowseModifiedFile_Click(object sender, RoutedEventArgs e) =>
        ModifiedFileTextBox.Text = await PickFilePathAsync("Select the modified file", false);

    private async void BrowseOutputPatchFile_Click(object sender, RoutedEventArgs e) =>
        OutputPatchFileTextBox.Text = await SaveFilePathAsync("Select where to save patch", "ups");

    private async void BrowseInspectPatchFile_Click(object sender, RoutedEventArgs e)
    {
        _inspectStorageFile = await PickStorageFileAsync("Select a patch", true);
        InspectPatchFileTextBox.Text = _inspectStorageFile?.TryGetLocalPath() ?? _inspectStorageFile?.Name ?? string.Empty;
    }

    private async void ApplyPatch_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PatchTargetFileTextBox.Text) || !File.Exists(PatchTargetFileTextBox.Text))
        {
            await ShowMessageAsync("Target file does not exist.");
            return;
        }

        if (_patchStorageFile == null &&
            (string.IsNullOrWhiteSpace(PatchFileTextBox.Text) || !File.Exists(PatchFileTextBox.Text)))
        {
            await ShowMessageAsync("Patch file does not exist.");
            return;
        }

        UPSfile upsFile;
        try
        {
            var patchBytes = await ReadStorageFileBytesAsync(_patchStorageFile, PatchFileTextBox.Text);
            upsFile = await Task.Run(() => new UPSfile(patchBytes));
        }
        catch
        {
            await ShowMessageAsync("Error opening patch file. Verify selected file paths.");
            return;
        }

        if (!upsFile.ValidPatch)
        {
            await ShowMessageAsync("The patch is corrupt.");
            return;
        }

        SetPatchingState(true);
        PatchStatusText.Text = "Verifying file…";
        PatchProgressBar.Value = 0;

        bool validToApply;
        try
        {
            using var cts = new CancellationTokenSource();
            _patchCts = cts;
            var verifyProgress = new Progress<double>(pct =>
            {
                PatchProgressBar.Value = pct;
                PatchStatusText.Text = $"Verifying… {pct:F0}%";
            });
            var targetPath = PatchTargetFileTextBox.Text;
            validToApply = await Task.Run(async () =>
                await upsFile.ValidToApplyAsync(targetPath, cts.Token, verifyProgress));
        }
        catch (OperationCanceledException)
        {
            PatchStatusText.Text = "Cancelled.";
            SetPatchingState(false);
            return;
        }
        catch
        {
            await ShowMessageAsync("Error reading target file.");
            SetPatchingState(false);
            return;
        }
        finally
        {
            _patchCts = null;
        }

        if (OnMismatchCancelRadio.IsChecked == true && !validToApply)
        {
            await ShowMessageAsync("The patch does not match the file. Patching canceled.");
            SetPatchingState(false);
            return;
        }

        if (OnMismatchAskRadio.IsChecked == true && !validToApply)
        {
            var shouldContinue = await ConfirmAsync("The patch does not match the file. Patch anyway?");
            if (!shouldContinue)
            {
                SetPatchingState(false);
                return;
            }
        }

        if (OnMismatchWarnRadio.IsChecked == true && !validToApply)
        {
            await ShowMessageAsync("The patch does not match the file. Patching anyway.");
        }

        if (CreateBackupCheckBox.IsChecked == true)
        {
            PatchStatusText.Text = "Creating backup…";
            PatchProgressBar.Value = 0;
            var backupProgress = new Progress<double>(pct =>
            {
                PatchProgressBar.Value = pct;
                PatchStatusText.Text = $"Backing up… {pct:F0}%";
            });
            try
            {
                using var backupCts = new CancellationTokenSource();
                _patchCts = backupCts;
                var targetForBackup = PatchTargetFileTextBox.Text;
                await Task.Run(async () => await CreateBackupAsync(targetForBackup, backupProgress, backupCts.Token));
            }
            catch (OperationCanceledException)
            {
                PatchStatusText.Text = "Cancelled.";
                SetPatchingState(false);
                return;
            }
            catch
            {
                await ShowMessageAsync("Failed to create backup. Patching cancelled.");
                SetPatchingState(false);
                return;
            }
            finally
            {
                _patchCts = null;
            }
        }

        PatchStatusText.Text = "Patching…";
        PatchProgressBar.Value = 0;

        var progress = new Progress<double>(pct =>
        {
            PatchProgressBar.Value = pct;
            PatchStatusText.Text = $"Patching… {pct:F0}%";
        });

        try
        {
            using var cts = new CancellationTokenSource();
            _patchCts = cts;
            var targetPath = PatchTargetFileTextBox.Text;
            await Task.Run(async () =>
                await upsFile.ApplyAsync(targetPath, targetPath, progress, cts.Token));
        }
        catch (OperationCanceledException)
        {
            PatchStatusText.Text = "Cancelled.";
            SetPatchingState(false);
            return;
        }
        catch
        {
            await ShowMessageAsync("Error applying patch. The original file may be unmodified.");
            SetPatchingState(false);
            return;
        }
        finally
        {
            _patchCts = null;
        }

        SetPatchingState(false);
        PatchProgressBar.Value = 100;
        PatchStatusText.Text = "Done.";
        await ShowMessageAsync("Patching has been done.");
        PatchProgressBar.Value = 0;
        PatchStatusText.Text = string.Empty;
    }

    private void CancelPatch_Click(object sender, RoutedEventArgs e)
    {
        _patchCts?.Cancel();
    }

    private void SetPatchingState(bool isPatching)
    {
        ApplyPatchButton.IsEnabled = !isPatching;
        CancelPatchButton.IsVisible = isPatching;
        PatchProgressPanel.IsVisible = isPatching;
        PatchTargetFileTextBox.IsEnabled = !isPatching;
        PatchFileTextBox.IsEnabled = !isPatching;
    }

    private async void CreatePatch_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(OriginalFileTextBox.Text) || !File.Exists(OriginalFileTextBox.Text))
        {
            await ShowMessageAsync("Original file does not exist.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ModifiedFileTextBox.Text) || !File.Exists(ModifiedFileTextBox.Text))
        {
            await ShowMessageAsync("Modified file does not exist.");
            return;
        }

        if (string.IsNullOrWhiteSpace(OutputPatchFileTextBox.Text))
        {
            await ShowMessageAsync("Output patch file is missing.");
            return;
        }

        try
        {
            var original = await File.ReadAllBytesAsync(OriginalFileTextBox.Text);
            var modified = await File.ReadAllBytesAsync(ModifiedFileTextBox.Text);
            var upsFile = new UPSfile(original, modified);
            upsFile.WriteToFile(OutputPatchFileTextBox.Text);
        }
        catch
        {
            await ShowMessageAsync("Error creating patch. Verify selected file paths.");
            return;
        }

        await ShowMessageAsync("Patch has been created.");
    }

    private async void ReadPatchData_Click(object sender, RoutedEventArgs e)
    {
        if (_inspectStorageFile == null &&
            (string.IsNullOrWhiteSpace(InspectPatchFileTextBox.Text) || !File.Exists(InspectPatchFileTextBox.Text)))
        {
            await ShowMessageAsync("Patch does not exist.");
            return;
        }

        try
        {
            var patchBytes = await ReadStorageFileBytesAsync(_inspectStorageFile, InspectPatchFileTextBox.Text);
            var details = await Task.Run(() =>
            {
                var upsFile = new UPSfile(patchBytes);
                return upsFile.GetData();
            });

            InspectOutputTextBox.Text = BuildPatchData(details);
        }
        catch
        {
            await ShowMessageAsync("Error reading patch data.");
        }
    }

    private void ClearPatchTab_Click(object sender, RoutedEventArgs e)
    {
        PatchTargetFileTextBox.Text = string.Empty;
        PatchFileTextBox.Text = string.Empty;
        _patchStorageFile = null;
        CreateBackupCheckBox.IsChecked = true;
        OnMismatchCancelRadio.IsChecked = true;
    }

    private void ClearCreateTab_Click(object sender, RoutedEventArgs e)
    {
        OriginalFileTextBox.Text = string.Empty;
        ModifiedFileTextBox.Text = string.Empty;
        OutputPatchFileTextBox.Text = string.Empty;
    }

    private void ClearInspectTab_Click(object sender, RoutedEventArgs e)
    {
        InspectPatchFileTextBox.Text = string.Empty;
        InspectOutputTextBox.Text = string.Empty;
        _inspectStorageFile = null;
    }

    /// <summary>
    /// Reads all bytes from a storage file via OpenReadAsync() if available,
    /// otherwise falls back to reading the file at the given path.
    /// Using OpenReadAsync() is the Avalonia-recommended approach and avoids
    /// platform-specific path encoding issues (e.g. spaces on Windows).
    /// </summary>
    private static async Task<byte[]> ReadStorageFileBytesAsync(IStorageFile storageFile, string fallbackPath)
    {
        if (storageFile != null)
        {
            await using var stream = await storageFile.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }

        return await File.ReadAllBytesAsync(fallbackPath);
    }

    private static string BuildPatchData(int[,] details)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Offsets\tLengths");
        for (var i = 0; i < details.GetLength(0); i++)
        {
            sb.AppendLine($"{Convert.ToString(details[i, 0], 16).ToUpper()}\t{Convert.ToString(details[i, 1], 16).ToUpper()}");
        }

        return sb.ToString();
    }

    private static async Task CreateBackupAsync(string filePath, IProgress<double> progress = null, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var backupPath = Path.Combine(directory, fileName + ".bak");

        var index = 1;
        while (File.Exists(backupPath))
        {
            backupPath = Path.Combine(directory, fileName + index + ".bak");
            index++;
        }

        var totalBytes = (double)new FileInfo(filePath).Length;
        long copied = 0;
        var lastReportedPct = -1;
        const int bufferSize = 1024 * 1024; // 1 MB chunks

        using var source = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var dest = new FileStream(backupPath, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize, FileOptions.Asynchronous);

        var buffer = new byte[bufferSize];
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            await dest.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            copied += bytesRead;
            if (progress != null && totalBytes > 0)
            {
                var pct = (int)(copied / totalBytes * 100.0);
                if (pct != lastReportedPct)
                {
                    progress.Report(pct);
                    lastReportedPct = pct;
                }
            }
        }

        await dest.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Returns a local path string for non-patch files where a path is required (target ROM, original, modified).</summary>
    private async Task<string> PickFilePathAsync(string title, bool upsOnly)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = upsOnly
                ? new[] { new FilePickerFileType("UPS files") { Patterns = new[] { "*.ups" } } }
                : null
        });

        return files.FirstOrDefault()?.TryGetLocalPath() ?? string.Empty;
    }

    /// <summary>Returns the IStorageFile directly so callers can use OpenReadAsync().</summary>
    private async Task<IStorageFile> PickStorageFileAsync(string title, bool upsOnly)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = upsOnly
                ? new[] { new FilePickerFileType("UPS files") { Patterns = new[] { "*.ups" } } }
                : null
        });

        return files.FirstOrDefault();
    }

    private async Task<string> SaveFilePathAsync(string title, string extension)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            DefaultExtension = extension,
            FileTypeChoices = new[] { new FilePickerFileType("UPS file") { Patterns = new[] { "*.ups" } } }
        });

        return file?.TryGetLocalPath() ?? string.Empty;
    }

    private async Task ShowMessageAsync(string message)
    {
        var okButton = new Button
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Content = "OK",
            Width = 84
        };

        var dialog = new Window
        {
            Width = 460,
            Height = 180,
            MinWidth = 360,
            MinHeight = 150,
            Title = "Nintenlord UPS patcher",
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(16),
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                    okButton
                }
            }
        };

        okButton.Click += (_, _) => dialog.Close();
        await dialog.ShowDialog(this);
    }

    private async Task<bool> ConfirmAsync(string message)
    {
        var result = false;
        var dialog = new Window
        {
            Width = 500,
            Height = 190,
            MinWidth = 390,
            MinHeight = 160,
            Title = "Confirm",
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var okButton = new Button { Content = "Patch", Width = 90 };
        var cancelButton = new Button { Content = "Cancel", Width = 90 };

        okButton.Click += (_, _) =>
        {
            result = true;
            dialog.Close();
        };

        cancelButton.Click += (_, _) => dialog.Close();

        dialog.Content = new StackPanel
        {
            Margin = new Avalonia.Thickness(16),
            Spacing = 14,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                    Spacing = 8,
                    Children = { okButton, cancelButton }
                }
            }
        };

        await dialog.ShowDialog(this);
        return result;
    }
}
